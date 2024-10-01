namespace ItemBuilder;


using Sandbox;
using System;

[CodeGenerator(CodeGeneratorFlags.WrapMethod | CodeGeneratorFlags.Instance, "OnInvoked")]
public class MyAttribute : Attribute { }


public class CodeGenExample
{
	[MyAttribute]
	public void DoInvoked()
	{
		Log.Info( "DoInvoked" );
	}

	internal void OnInvoked(WrappedMethod m, params object[] args)
	{
		Log.Info( "OnInvoked" );
		m.Resume();
	}
}
