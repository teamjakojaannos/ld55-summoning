using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D {
    [Export]
    public float MoveSpeed = 32.0f;

    [Export]
    public AnimatedSprite2D Sprite;

    [Export]
    public Overlay Blackout;

    [Export]
    public float FadeSmoothIn = 0.05f;

    [Export]
    public float FadeSmoothOut = 0.1f;

    [Export]
    public float FadeSmoothInTime = 3.0f;

    [Export]
    public float MaxFade = 200.0f;
    [Export]
    public float MaxFactor = 10.0f;

    [Export]
    public AudioStreamPlayer CombatMusic;

    [Export]
    public AudioStreamPlayer EnterCombatMusic;


    [Export]
    public AudioStreamPlayer ExplocationMusic;

    private bool _isInCinematic = false;
    public bool IsInCinematic {
        get => _isInCinematic;
        set {
            _isInCinematic = value;
            inputStack.Clear();
        }
    }

    private List<InputDirection> inputStack = new();


    private float fadeSmoothness  = 1.0f;
    private float fadeDistance = 25.0f;
    private float darkDistanceFactor = 25.0f;
    private float targetFade = 25.0f;
    private float targetDarkDistanceFactor = 25.0f;

    private bool isTransitioning;

    public override void _Ready() {
        fadeDistance = MaxFade;
        darkDistanceFactor = MaxFactor;
        targetFade = fadeDistance;
        targetDarkDistanceFactor = darkDistanceFactor;

        fadeSmoothness = FadeSmoothIn;
        isTransitioning = false;
        EnterCombatMusic.Stop();
        ExplocationMusic.Play();
        CombatMusic.Stop();
    }

    public override void _EnterTree() {
        var startNode = GetTree().Root.FindChild("PlayerStart", true, false);
        if (startNode is Marker2D startMarker) {
            GlobalPosition = startMarker.GlobalPosition;
        } else {
            GD.PushError("Could not find PlayerStart");
        }
    }

    public override void _Process(double delta) {
        if (Blackout != null) {
            fadeDistance = Mathf.Lerp(fadeDistance, targetFade, fadeSmoothness);
            darkDistanceFactor = Mathf.Lerp(darkDistanceFactor, targetDarkDistanceFactor, fadeSmoothness);
            Blackout.Visible = true;

            (Blackout.Material as ShaderMaterial)?.SetShaderParameter("bright_dist", fadeDistance);
            (Blackout.Material as ShaderMaterial)?.SetShaderParameter("darkness_dist_factor", darkDistanceFactor);
        }

        Vector2I inputDirection = inputStack.Count == 0 ? Vector2I.Zero : InputDirectionAsVector(inputStack[0]);

        CheckInput("move_left", InputDirection.Left);
        CheckInput("move_right", InputDirection.Right);
        CheckInput("move_up", InputDirection.Up);
        CheckInput("move_down", InputDirection.Down);

        if (isTransitioning || IsInCinematic) {
            Velocity = Vector2.Zero;
            Sprite.Play("idle");
            return;
        }

        Velocity = MoveSpeed * new Vector2(inputDirection.X, inputDirection.Y);

        if (Mathf.Abs(Velocity.X) > 0.001f && Sprite != null) {
            Sprite.FlipH = Velocity.X < 0.0f;
        }

        if (Mathf.Abs(Velocity.LengthSquared()) > 0.001f) {
            if (Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y)) {
                Sprite.Play("walk_side");
            } else if (Velocity.Y < 0.0f) {
                Sprite.Play("walk_up");
            } else {
                Sprite.Play("walk_down");
            }
        } else {
            Sprite.Play("idle");
        }

        MoveAndSlide();
    }

    // Pass null target to perform only the fade-in-out without any music or level transitions
    public void StartEncounter(EncounterTrigger target) {
        Sprite?.Play("blink");
        targetFade = 0.0f;
        targetDarkDistanceFactor = 0.0f;
        fadeSmoothness = FadeSmoothIn;
        inputStack.Clear();
        Velocity = Vector2.Zero;

        if (target != null) {
            EnterCombatMusic.Play();
            ExplocationMusic.Stop();
            CombatMusic.Stop();
        }

        isTransitioning = true; // trans rights are human rights

        GetTree().CreateTimer(FadeSmoothInTime).Timeout += () => {
            targetFade = MaxFade;
            targetDarkDistanceFactor = MaxFactor;
            fadeSmoothness = FadeSmoothOut;

            if (target != null) {
                ExplocationMusic.Stop();
                CombatMusic.Play();

                var cardGameNode = GetTree().Root.FindChild("CardGameLayer", true, false);
                if (cardGameNode is not CanvasLayer cardGame) {
                    GD.PushError("Could not find card game layer");
                    return;
                }

                cardGame.Visible = true;
                // FIXME: assumes all level scene roots are called "World"
                GetNode<Node2D>("/root/Root/World").Visible = false;
                Sprite.Visible = false;
            }

            GetTree().CreateTimer(FadeSmoothInTime / 2.0f).Timeout += () => {
                isTransitioning = false;
            };
        };
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
