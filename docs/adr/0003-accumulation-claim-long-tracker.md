# Accumulation lives on the Quest; progress values are long in v0.1

Report application modes (Sum, HighWater, Flag) are declared on the Quest definition so gameplay only reports facts. Claim supports once-per-step and repeat-with-reset; Idle-specific sibling resets stay in the game board. Progress and thresholds use `long` in v0.1 (BigInteger deferred). The public Runtime API is a pure C# Tracker; any MonoBehaviour facade belongs in Samples~, not as the core API.
