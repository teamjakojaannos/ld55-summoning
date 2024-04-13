using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D {
    [Export]
    public float MoveSpeed = 32.0f;

    private List<InputDirection> inputStack = new();

    public override void _Process(double delta) {
        Vector2I inputDirection = inputStack.Count == 0 ? Vector2I.Zero : InputDirectionAsVector(inputStack[0]);

        CheckInput("move_left", InputDirection.Left);
        CheckInput("move_right", InputDirection.Right);
        CheckInput("move_up", InputDirection.Up);
        CheckInput("move_down", InputDirection.Down);

        Velocity = MoveSpeed * new Vector2(inputDirection.X, inputDirection.Y);
        MoveAndSlide();
    }

    private void CheckInput(StringName action, InputDirection direction) {
        if (Input.IsActionJustPressed(action)) {
            inputStack = inputStack.Prepend(direction).ToList();
        }

        if (Input.IsActionJustReleased(action)) {
            inputStack.Remove(direction);
        }
    }

    private static Vector2I InputDirectionAsVector(InputDirection direction) {
        return direction switch {
            InputDirection.Left => Vector2I.Left,
            InputDirection.Right => Vector2I.Right,
            InputDirection.Up => Vector2I.Up,
            InputDirection.Down => Vector2I.Down,
            _ => throw new NotSupportedException("The above cases are exhaustive. There be dragons if this is ever reached."),
        };
    }

    private enum InputDirection {
        Left,
        Right,
        Up,
        Down,
    }
}
