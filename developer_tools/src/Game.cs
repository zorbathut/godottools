
using Godot;

public abstract class Game
{
    public abstract void Process();

    public Comp.PlayerInput GetUserInput()
    {
        // someday this will get more complicated when UI happens
        return new Comp.PlayerInput() {
            dx = Mathf.RoundToInt(Input.GetActionStrength("dpad_right") - Input.GetActionStrength("dpad_left")),
            dy = Mathf.RoundToInt(Input.GetActionStrength("dpad_down") - Input.GetActionStrength("dpad_up")),
            jump = Input.IsActionPressed("jump"),
            attack = Input.IsActionPressed("attack"),
        };
    }
}
