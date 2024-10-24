using Editor;
using Sandbox;

namespace ItemBuilder;

[EditorTool("ItemBuilder")]
[Title( "Item Builder" )]
[Icon("engineering")]
[Alias( "item" )]
[Group( "0" )]
public class ItemBuilder : EditorTool
{
	public override void OnEnabled()
	{
		var window = new WidgetWindow( SceneOverlay );

		window.WindowTitle = "Item Builder";
		window.Layout = Layout.Column();
		
		window.MinimumSize = new Vector2( 128, 240 );
		window.MinimumHeight = 128.0f;
		window.MaximumWidth = 480.0f;

		window.Layout.Add(new ItemInfoWidget( window ) );
		window.Layout.Add(new ItemAbilitiesWidget( window ) );

		AddOverlay( window );
	}

	public override void OnUpdate()
	{
		base.OnUpdate();

		var traceResult = Scene.Trace.Ray( Gizmo.CurrentRay, 4096 )
			.UseRenderMeshes( true )
			.UsePhysicsWorld( false )
			.Run();

		if ( traceResult.Hit )
		{
			using ( Gizmo.Scope( "cursor" ) )
			{
				//Gizmo.Transform = new Transform( traceResult.HitPosition.SnapToGrid(4.0f), Rotation.LookAt( traceResult.Normal ) );
				//Gizmo.Draw.SolidSphere(Vector3.Zero, 8.0f);
			}
		}
	}
	
}
