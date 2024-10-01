using System;

namespace Editor.FoliagePainter;
using Foliage;
[EditorTool] // this class is an editor tool
[Title( "Foliage" )] // title of your tool
[Icon( "grass" )] // icon name from https://fonts.google.com/icons?selected=Material+Icons
public class FoliagePainter : EditorTool
{
	private float PaintProgress { get; set; } = 0f;
	public FoliageSettings FoliageSettings { get; private set; } = new();
	public override void OnEnabled()
	{
		AllowGameObjectSelection = false;
		if ( GetSelectedComponent<FoliageRenderer>() == null )
		{
			Selection.Clear();
			var first = Scene.GetAllComponents<FoliageRenderer>().FirstOrDefault();
			if ( first is not null ) Selection.Add( first.GameObject );
		}
		
		var foliageSettings = new FoliageSettingsWidgetWindow( SceneOverlay, EditorUtility.GetSerializedObject( FoliageSettings ) );
		AddOverlay( foliageSettings, TextFlag.RightBottom, 10 );
	}

	private bool Painted { get; set; } = false;

	public override void OnUpdate()
	{
		var tr = Scene.Trace.Ray( Gizmo.CurrentRay, 5000 )
			.UseRenderMeshes( true )
			.UsePhysicsWorld( false )
			.WithoutTags( "invisible" )
			.Run();


		if ( tr.Hit )
		{
			using ( Gizmo.Scope( "cursor" ) )
			{
				Gizmo.Transform = new Transform( tr.HitPosition, Rotation.LookAt( tr.Normal ) );
				Gizmo.Draw.LineCircle( 0, FoliageSettings.Size );

			}

			if ( Gizmo.IsLeftMouseDown && Application.KeyboardModifiers.HasFlag( Sandbox.KeyboardModifiers.Shift ) )
			{
				var paintTarget = GetSelectedComponent<FoliageRenderer>();
				if ( paintTarget == null ) { return; }

				if ( FoliageSettings.EraseOnlySelectedFoliage )
				{
					paintTarget.EraseFoliage( tr.HitPosition, FoliageSettings.Size, FoliageSettings.Foliage.ResourceId );
				}
				else
				{
					paintTarget.EraseFoliage( tr.HitPosition, FoliageSettings.Size, 0 );
				}
				
				Painted = true;
			}

			if ( !Gizmo.IsLeftMouseDown && Painted )
			{
				Painted = false;
				SceneEditorSession.Active.FullUndoSnapshot("Paint Grass");
				
				var paintTarget = GetSelectedComponent<FoliageRenderer>();
				if ( paintTarget == null ) { return; }
				//paintTarget.UpdateRenderers();
			}

			if ( Gizmo.IsLeftMouseDown && !Application.KeyboardModifiers.HasFlag( Sandbox.KeyboardModifiers.Shift ))
			{
				if ( GetSelectedComponent<FoliageRenderer>() == null )
				{
					Selection.Clear();
					var first = Scene.GetAllComponents<FoliageRenderer>().FirstOrDefault();
					if ( first is not null ) Selection.Add( first.GameObject );
					return;
				}
				
				if ( FoliageSettings.Foliage == null ) { return;}
				PaintProgress += Time.Delta * FoliageSettings.PaintSpeed * 3f;
				if ( PaintProgress >= 1 )
				{
					PaintProgress = 0;

					var paintTarget = GetSelectedComponent<FoliageRenderer>();
					if ( paintTarget == null ) { return; }
					
					var randomPos =
						new Vector3( Random.Shared.Float( FoliageSettings.Size * -1, FoliageSettings.Size ),
							Random.Shared.Float( FoliageSettings.Size * -1, FoliageSettings.Size ), FoliageSettings.Size ).RotateAround( Vector3.Zero,
							Rotation.LookAt( tr.Normal ) * Rotation.FromPitch( 90f ) );

					randomPos = randomPos.ClampLength( FoliageSettings.Size );
					
					var paintTr = Scene.Trace.Ray( new Ray( tr.HitPosition + randomPos,Rotation.LookAt( tr.Normal ) * Rotation.FromPitch( 90f ).Down ), FoliageSettings.Size+2 )
						.UseRenderMeshes( true )
						.UsePhysicsWorld( false )
						.WithoutTags( "invisible" )
						.Run();

					if ( paintTr.Hit )
					{
						if ( 1-((paintTr.Normal.Dot( new Vector3( 0, 0, 1 ) ) + 1f) / 2f) > FoliageSettings.Foliage.MaxNormal / 180 )
						{
							return;
						}
						
						
						var randomRot =
							Rotation.FromYaw( FoliageSettings.Foliage.RandomRoll ? Random.Shared.Float( 0, 360 ) : 0 );
						if ( FoliageSettings.Foliage.RandomRotation )
						{
							randomRot = Rotation.Random;
						}
						
						var transform = new Transform( paintTr.HitPosition+(tr.Normal* (FoliageSettings.Foliage.ZOffset.GetValue())) );
						if ( FoliageSettings.Foliage.AlignToNormal )
						{
							transform = transform.WithRotation( Rotation.LookAt( paintTr.Normal )*Rotation.FromPitch( 90f )*randomRot );
						}
						else
						{
							transform = transform.WithRotation( randomRot );
						}
						
						transform = transform.WithScale(
							FoliageSettings.Foliage.Scale.GetValue());
					
						paintTarget.PaintFoliage( FoliageSettings.Foliage ,transform );
						Painted = true;
					}

				}
			}
			
		}
	}
}
