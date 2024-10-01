using System;
using System.Text.Json.Serialization;
using Sandbox.Engine.Settings;

namespace Foliage;


public class FoliageRenderer : Component, Component.ExecuteInEditor
{
	public List<FoliageSceneObject> Renderers { get; set; } = new();
	[Property,Hide] Dictionary<int,List<Transform>> FoliageRenderers { get; set; } = new();
	[Property] OptionsWidget Options { get; set; }
	public void PaintFoliage(FoliageResource foliage, Transform transform)
	{
		if ( FoliageRenderers.ContainsKey( foliage.ResourceId ) )
		{
			FoliageRenderers[foliage.ResourceId].Add( transform );
			UpdateRenderers();
		}
		else
		{
			FoliageRenderers.Add( foliage.ResourceId, new List<Transform> { transform } );
			FoliageRenderers[foliage.ResourceId].Add( transform );
			UpdateRenderers();
		}
	}

	public void EraseFoliage( Vector3 position,float size, int id )
	{
		List<Transform> toRemove = new();

		if ( id != 0 )
		{
			foreach ( var testPos in FoliageRenderers[id] )
			{
				if ( testPos.Position.DistanceSquared( position ) <= size * size )
				{
					toRemove.Add( testPos );
				}
			}
			
			foreach ( var remove in toRemove )
			{
				FoliageRenderers[id].Remove( remove );
			}
			return;
		}
		
		foreach ( var testVal in FoliageRenderers )
		{
			foreach ( var testPos in testVal.Value )
			{
				if ( testPos.Position.DistanceSquared( position ) <= size * size )
				{
					toRemove.Add( testPos );
				}
			}

			foreach ( var remove in toRemove )
			{
				testVal.Value.Remove( remove );
			}
		}
		
		
	}

	protected override void OnStart()
	{
		foreach ( var val in Scene.SceneWorld.SceneObjects )
		{
			if ( val.GetType() == typeof(FoliageSceneObject) )
			{
				val.Delete();
			}
		}
		
		UpdateRenderers();
		
		
	}

	protected override void OnUpdate()
	{
		
		
	}

	
	public void UpdateRenderers()
	{
		foreach ( var val in Renderers )
		{
			val.Delete();
			
		}

		
		
		Renderers.Clear();
		foreach ( var folRenderer in FoliageRenderers )
		{
			var transformArray = folRenderer.Value.ToArray();
			var folInstance = ResourceLibrary.Get<FoliageResource>( folRenderer.Key );
			
			
			var folSceneObject = new FoliageSceneObject( GameObject.Scene.SceneWorld, folInstance.Model )
			{
				Transforms = transformArray
			};

			
			
			Renderers.Add( folSceneObject );




		}
		
	}
	public void ClearAll()
	{
		foreach ( var val in FoliageRenderers )
		{
			val.Value.Clear();
		}
		UpdateRenderers();
	}
	
	public class OptionsWidget { }
}
public class FoliageSceneObject : SceneCustomObject
{
	public Model RenderModel;
	public Transform[] Transforms = Array.Empty<Transform>();
	public FoliageSceneObject(SceneWorld sceneWorld, Model renderModel) : base(sceneWorld)
	{
		RenderModel = renderModel;
	}

	
	public void AddTransform(Transform transform)
	{
		//Transforms.Add( transform );
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();
		Graphics.DrawModelInstanced( RenderModel, Transforms.AsSpan() );
		
		
	}
}
