// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Simulation;

namespace Spellbound.Places.Arrangement {
    internal sealed class ArrangementComposition {
        public const string EmptiedDomainReason = "no part fits the faces required by its neighbours";

        private readonly ArrangementDescription _arrangement;
        private readonly CompositionGrid _grid;
        private readonly SeededStream _collapseOrder;
        private readonly SeededStream _guaranteedPlacements;
        private readonly SeededStream[] _strataStreams;
        private readonly string[] _partNames;
        private readonly Dictionary<string, int> _partIndices = new(StringComparer.Ordinal);
        private readonly Dictionary<int, List<CellRegion>> _allowedRegions = new();
        private readonly Dictionary<int, ResolvedPool> _cellPools = new();
        private readonly ResolvedPool[] _strataPools;
        private readonly List<CellContradiction> _contradictions = new();
        private int _ordinaryCellsAttempted;
        private int _ordinaryCellsEmpty;

        public ArrangementComposition(ArrangementDescription arrangement, ulong seed) {
            _arrangement = arrangement;

            var names = new List<string>();
            var faces = new List<PartFaces>();

            foreach (var part in arrangement.Parts) {
                if (part == null || part.partName == null || _partIndices.ContainsKey(part.partName)) {
                    continue;
                }

                _partIndices.Add(part.partName, names.Count);
                names.Add(part.partName);
                faces.Add(part.faces);
            }

            _partNames = names.ToArray();
            _grid = new CompositionGrid(arrangement.GridSize, arrangement.Strata.Count, faces.ToArray());

            _collapseOrder = new SeededStream(seed);
            _guaranteedPlacements = _collapseOrder.Fork(ulong.MaxValue);
            _strataStreams = new SeededStream[_grid.StratumCount];
            _strataPools = new ResolvedPool[_grid.StratumCount];

            for (var stratum = 0; stratum < _grid.StratumCount; stratum++) {
                _strataStreams[stratum] = _collapseOrder.Fork((ulong)stratum);
                _strataPools[stratum] = ResolvePool(arrangement.Strata[stratum]?.pool, new CellAddress(0, 0, stratum));
            }
        }

        public CompositionResult Compose() {
            ReadAreaRules();
            PlaceCentre();
            PlaceFixedCells();

            for (var stratum = 0; stratum < _grid.StratumCount; stratum++) {
                PlaceOneOfParts(stratum);
            }

            SolveOrdinaryCells();

            var cells = new CompositionCell[_grid.CellCount];

            for (var index = 0; index < cells.Length; index++) {
                cells[index] = _grid.CellAt(index, _partNames);
            }

            return new CompositionResult(_grid.Size, _grid.StratumCount, cells, _contradictions,
                _ordinaryCellsAttempted, _ordinaryCellsEmpty);
        }

        private void ReadAreaRules() {
            foreach (var rule in _arrangement.Rules) {
                switch (rule) {
                    case WeightedPoolRule poolRule:
                        if (!_grid.Contains(poolRule.cell)) {
                            Report(poolRule.cell, "the pool rule's cell is outside the grid");
                            break;
                        }

                        _cellPools[_grid.IndexOf(poolRule.cell)] = ResolvePool(poolRule.pool, poolRule.cell);
                        break;
                    case AllowedRegionRule regionRule:
                        if (!TryFindPart(regionRule.partName, regionRule.region.minimum, "region", out var part)) {
                            break;
                        }

                        if (!_allowedRegions.TryGetValue(part, out var regions)) {
                            regions = new List<CellRegion>();
                            _allowedRegions.Add(part, regions);
                        }

                        regions.Add(regionRule.region);
                        break;
                }
            }
        }

        private void PlaceCentre() {
            if (_grid.StratumCount == 0) {
                return;
            }

            var centrePool = ResolvePool(_arrangement.CentrePool, CentreAddress(0));

            if (centrePool.Count == 0) {
                for (var stratum = 0; stratum < _grid.StratumCount; stratum++) {
                    _grid.MarkVoidCentre(_grid.IndexOf(CentreAddress(stratum)));
                }

                return;
            }

            var index = _grid.IndexOf(CentreAddress(0));
            var required = _grid.RequiredAt(index);
            var masks = new int[centrePool.Count];
            var eligibleEntries = new List<int>();
            var eligibleWeights = new List<float>();

            for (var entry = 0; entry < masks.Length; entry++) {
                masks[entry] = _grid.ValidRotationMask(centrePool.Parts[entry], required);

                if (masks[entry] != 0) {
                    eligibleEntries.Add(entry);
                    eligibleWeights.Add(centrePool.Weights[entry]);
                }
            }

            if (eligibleEntries.Count == 0) {
                Report(CentreAddress(0), "no centre part fits the faces required by its neighbours");
                var fallback = _guaranteedPlacements.NextInteger(0, centrePool.Count);
                _grid.Place(index, centrePool.Parts[fallback], 0, true);
                return;
            }

            var chosen = eligibleEntries[WeightedPick.Pick(_guaranteedPlacements, eligibleWeights)];
            _grid.Place(index, centrePool.Parts[chosen], PickRotation(_guaranteedPlacements, masks[chosen]), true);
        }

        private void PlaceFixedCells() {
            foreach (var rule in _arrangement.Rules) {
                if (!(rule is FixedCellRule fixedRule)) {
                    continue;
                }

                if (!_grid.Contains(fixedRule.cell)) {
                    Report(fixedRule.cell, "the fixed cell is outside the grid");
                    continue;
                }

                if (!TryFindPart(fixedRule.partName, fixedRule.cell, "fixed", out var part)) {
                    continue;
                }

                var index = _grid.IndexOf(fixedRule.cell);

                if (!_grid.IsFree(index)) {
                    Report(fixedRule.cell, "the fixed cell is already taken by the centre or an earlier fixed cell");
                    continue;
                }

                var mask = _grid.ValidRotationMask(part, _grid.RequiredAt(index));
                int rotationSteps;

                if (fixedRule.rotationSteps < 0) {
                    if (mask == 0) {
                        Report(fixedRule.cell, "the fixed part '" + fixedRule.partName
                                               + "' fits no rotation against the faces required by its neighbours");
                        rotationSteps = 0;
                    } else {
                        rotationSteps = PickRotation(_guaranteedPlacements, mask);
                    }
                } else {
                    rotationSteps = fixedRule.rotationSteps % 4;

                    if ((mask & (1 << rotationSteps)) == 0) {
                        Report(fixedRule.cell, "the fixed part '" + fixedRule.partName + "' at rotation "
                                               + rotationSteps + " does not fit the faces required by its neighbours");
                    }
                }

                _grid.Place(index, part, rotationSteps, false);
            }
        }

        private void PlaceOneOfParts(int stratum) {
            var oneOfParts = _arrangement.Strata[stratum]?.oneOfParts;

            if (oneOfParts == null) {
                return;
            }

            var stratumAddress = new CellAddress(0, 0, stratum);

            foreach (var partName in oneOfParts) {
                if (!TryFindPart(partName, stratumAddress, "one-of", out var part)) {
                    continue;
                }

                var candidates = FreeNonCentreCells(stratum);

                if (candidates.Count == 0) {
                    Report(stratumAddress, "the one-of part '" + partName + "' has no free cell in its stratum");
                    continue;
                }

                Shuffle(candidates, _guaranteedPlacements);
                var placed = false;

                foreach (var index in candidates) {
                    if (!IsPermitted(part, _grid.AddressOf(index))) {
                        continue;
                    }

                    var mask = _grid.ValidRotationMask(part, _grid.RequiredAt(index));

                    if (mask == 0) {
                        continue;
                    }

                    _grid.Place(index, part, PickRotation(_guaranteedPlacements, mask), false);
                    placed = true;
                    break;
                }

                if (!placed) {
                    Report(_grid.AddressOf(candidates[0]), "the one-of part '" + partName
                                                           + "' fits no free cell and is placed at an arbitrary one");
                    _grid.Place(candidates[0], part, 0, false);
                }
            }
        }

        private void SolveOrdinaryCells() {
            var worklist = new PropagationWorklist(_grid.CellCount);

            for (var index = 0; index < _grid.CellCount; index++) {
                if (!_grid.IsFree(index)) {
                    continue;
                }

                var stratum = _grid.AddressOf(index).stratum;
                var pool = _cellPools.TryGetValue(index, out var cellPool) ? cellPool : _strataPools[stratum];
                _grid.SetDomain(index, pool.DomainAt(_grid.AddressOf(index), IsPermitted));
                _ordinaryCellsAttempted++;
                worklist.Enqueue(index);
            }

            Propagate(worklist);

            while (true) {
                var chosen = LowestEntropyCell();

                if (chosen < 0) {
                    return;
                }

                var domain = _grid.Domain(chosen);

                if (domain.Entropy <= 0) {
                    _grid.SetDomain(chosen, null);
                    _ordinaryCellsEmpty++;
                    Report(_grid.AddressOf(chosen), EmptiedDomainReason);
                    continue;
                }

                var stream = _strataStreams[_grid.AddressOf(chosen).stratum];
                var eligibleEntries = new List<int>();
                var eligibleWeights = new List<float>();

                for (var entry = 0; entry < domain.Count; entry++) {
                    if (domain.RotationMask(entry) != 0) {
                        eligibleEntries.Add(entry);
                        eligibleWeights.Add(domain.Weight(entry));
                    }
                }

                var picked = eligibleEntries[WeightedPick.Pick(stream, eligibleWeights)];
                var rotationSteps = PickRotation(stream, domain.RotationMask(picked));
                _grid.Place(chosen, domain.Part(picked), rotationSteps, false);
                _grid.EnqueueOpenNeighbours(chosen, worklist);
                Propagate(worklist);
            }
        }

        private void Propagate(PropagationWorklist worklist) {
            while (worklist.TryDequeue(out var index)) {
                var domain = _grid.Domain(index);

                if (domain != null && domain.Refilter(_grid, _grid.RequiredAt(index))) {
                    _grid.EnqueueOpenNeighbours(index, worklist);
                }
            }
        }

        private int LowestEntropyCell() {
            var chosen = -1;
            var lowest = int.MaxValue;
            var ties = 0;

            for (var index = 0; index < _grid.CellCount; index++) {
                var domain = _grid.Domain(index);

                if (domain == null) {
                    continue;
                }

                if (domain.Entropy < lowest) {
                    lowest = domain.Entropy;
                    chosen = index;
                    ties = 1;
                } else if (domain.Entropy == lowest) {
                    ties++;

                    if (_collapseOrder.NextInteger(0, ties) == 0) {
                        chosen = index;
                    }
                }
            }

            return chosen;
        }

        private List<int> FreeNonCentreCells(int stratum) {
            var cells = new List<int>();

            for (var row = 0; row < _grid.Size; row++) {
                for (var column = 0; column < _grid.Size; column++) {
                    var index = _grid.IndexOf(new CellAddress(column, row, stratum));

                    if (_grid.IsFree(index) && !_grid.IsCentreColumn(index)) {
                        cells.Add(index);
                    }
                }
            }

            return cells;
        }

        private bool IsPermitted(int part, CellAddress cell) {
            if (!_allowedRegions.TryGetValue(part, out var regions)) {
                return true;
            }

            foreach (var region in regions) {
                if (region.Contains(cell)) {
                    return true;
                }
            }

            return false;
        }

        private ResolvedPool ResolvePool(IReadOnlyList<PoolEntry> pool, CellAddress reportAt) {
            var parts = new List<int>();
            var weights = new List<float>();

            if (pool != null) {
                foreach (var entry in pool) {
                    if (entry == null) {
                        continue;
                    }

                    if (TryFindPart(entry.partName, reportAt, "pool", out var part)) {
                        parts.Add(part);
                        weights.Add(entry.weight);
                    }
                }
            }

            return new ResolvedPool(parts.ToArray(), weights.ToArray());
        }

        private bool TryFindPart(string partName, CellAddress reportAt, string role, out int part) {
            if (partName != null && _partIndices.TryGetValue(partName, out part)) {
                return true;
            }

            Report(reportAt, "the " + role + " part '" + partName + "' is not one of the arrangement's parts");
            part = CompositionGrid.NoPart;
            return false;
        }

        private CellAddress CentreAddress(int stratum) {
            return new CellAddress(_grid.Centre, _grid.Centre, stratum);
        }

        private void Report(CellAddress cell, string reason) {
            _contradictions.Add(new CellContradiction(cell, reason));
        }

        private static int PickRotation(SeededStream stream, int mask) {
            var choice = stream.NextInteger(0, CellDomain.RotationCount(mask));

            for (var rotation = 0; rotation < 4; rotation++) {
                if ((mask & (1 << rotation)) == 0) {
                    continue;
                }

                if (choice == 0) {
                    return rotation;
                }

                choice--;
            }

            return 0;
        }

        private static void Shuffle(List<int> cells, SeededStream stream) {
            for (var index = cells.Count - 1; index > 0; index--) {
                var swap = stream.NextInteger(0, index + 1);
                (cells[index], cells[swap]) = (cells[swap], cells[index]);
            }
        }
    }
}
