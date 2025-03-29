using System;

namespace GameProject {
    public class AposNumber {
        public AposNumber(int value, int exp, int skip) {
            Value = value;
            Exp = exp;
            Skip = skip;
        }

        int Value { get; set; } // Value within the grid.
        int Exp { get; set; } // Size of the grid.
        int Skip { get; set; } // How many full grids were skipped to get to this grid.

        public override bool Equals(object obj) => this.Equals(obj as AposNumber);

        public bool Equals(AposNumber b) {
            if (b is null) return false;

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, b)) return true;

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != b.GetType()) return false;

            return this == b;
        }

        public override string ToString() {
            return $"({Value}, {Exp}, {Skip})";
        }

        public override int GetHashCode() => (Value, Exp, Skip).GetHashCode();

        public static bool operator <(AposNumber a, AposNumber b) {
            // 100, 0, 0   <   100, 1, 0   <   100, 2, 0
            // 200, 0, 0   ==  100, 1, 0
            // 300, 0, 0   >   100, 1, 0

            // 200, 0, 1   ==  100, 1, 0
            // 200, 0, 1   <  2000000000, 1, 0

            if (a.Exp == b.Exp) {
                if (a.Skip == b.Skip) {
                    return a.Value < b.Value;
                } else {
                    return a.Skip < b.Skip;
                }
            } else if (a.Exp < b.Exp) {
                float aP = MathF.Pow(2f, a.Exp - b.Exp);
                int aS = (int)(a.Skip * aP);

                if (aS == b.Skip) {
                    int aM = (int)(a.Skip % MathF.Pow(2f, b.Exp - a.Exp));
                    int aV = (int)(a.Value * aP + aM * (int.MaxValue * aP));
                    return aV < b.Value;
                } else {
                    return aS < b.Skip;
                }
            } else {
                float bP = MathF.Pow(2f, b.Exp - a.Exp);
                int bS = (int)(b.Skip * bP);

                if (bS == a.Skip) {
                    int bM = (int)(b.Skip % MathF.Pow(2f, a.Exp - b.Exp));
                    int bV = (int)(b.Value * bP + bM * (int.MaxValue * bP));
                    return bV < a.Value;
                } else {
                    return bS < a.Skip;
                }
            }
        }

        public static bool operator >(AposNumber a, AposNumber b) {
            if (a.Exp == b.Exp) {
                if (a.Skip == b.Skip) {
                    return a.Value > b.Value;
                } else {
                    return a.Skip > b.Skip;
                }
            } else if (a.Exp < b.Exp) {
                float aP = MathF.Pow(2f, a.Exp - b.Exp);
                int aS = (int)(a.Skip * aP);

                if (aS == b.Skip) {
                    int aM = (int)(a.Skip % MathF.Pow(2f, b.Exp - a.Exp));
                    int aV = (int)(a.Value * aP + aM * (int.MaxValue * aP));
                    return aV > b.Value;
                } else {
                    return aS > b.Skip;
                }
            } else {
                float bP = MathF.Pow(2f, b.Exp - a.Exp);
                int bS = (int)(b.Skip * bP);

                if (bS == a.Skip) {
                    int bM = (int)(b.Skip % MathF.Pow(2f, a.Exp - b.Exp));
                    int bV = (int)(b.Value * bP + bM * (int.MaxValue * bP));
                    return bV > a.Value;
                } else {
                    return bS > a.Skip;
                }
            }
        }

        public static bool operator ==(AposNumber a, AposNumber b) {
            if (a.Exp == b.Exp) {
                if (a.Skip == b.Skip) {
                    return a.Value == b.Value;
                } else {
                    return false;
                }
            } else if (a.Exp < b.Exp) {
                float aP = MathF.Pow(2f, a.Exp - b.Exp);
                int aS = (int)(a.Skip * aP);

                if (aS == b.Skip) {
                    int aM = (int)(a.Skip % MathF.Pow(2f, b.Exp - a.Exp));
                    int aV = (int)(a.Value * aP + aM * (int.MaxValue * aP));
                    return aV == b.Value;
                } else {
                    return false;
                }
            } else {
                float bP = MathF.Pow(2f, b.Exp - a.Exp);
                int bS = (int)(b.Skip * bP);

                if (bS == a.Skip) {
                    int bM = (int)(b.Skip % MathF.Pow(2f, a.Exp - b.Exp));
                    int bV = (int)(b.Value * bP + bM * (int.MaxValue * bP));
                    return bV == a.Value;
                } else {
                    return false;
                }
            }
        }

        public static bool operator !=(AposNumber a, AposNumber b) {
            return !(a == b);
        }
    }
}
