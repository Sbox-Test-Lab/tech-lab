namespace ItemBuilder;

public class InputActionFunction : InputFunction
{
	public enum InputState
	{
		Pressed,
		Down,
		Released
	}

	[Property, InputAction] public string Action { get; set; }
	[Property] public InputState State { get; set; } = InputState.Pressed;

	public delegate void InputActionActiveDelegate(string action, InputState state);
	[Property] public InputActionActiveDelegate InputActionActive { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		InputActionActive += OnInputActionActive;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if(IsStateActive(Action))
		{
			InputActionActive?.Invoke( Action, State );
		}
	}

	protected virtual void OnInputActionActive(string action, InputState state)
	{
		
	}

	public bool IsStateActive(string action)
	{
		return State switch
		{
			InputState.Pressed => Input.Pressed( action ),
			InputState.Down => Input.Down( action ),
			InputState.Released => Input.Released( action ),

			_ => throw new System.Exception( "Could not find input state for input action" )
		};
	}
}
