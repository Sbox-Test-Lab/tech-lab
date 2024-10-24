using Sandbox;
using System;
using System.Linq;

/// <summary>
/// Move an object around a path.
/// </summary>
[Title( "Motion Path" )]
public class MotionPath : Component, Component.ExecuteInEditor
{
	/// <summary>
	/// The object that moves around this path.
	/// </summary>
	[Property]
	public GameObject Target { get; set; }

	/// <summary>
	/// Normalized time between 0 and 1 for how far the target is on this path.
	/// </summary>
	[Property, Range( 0.0f, 1.0f )]
	public float Time { get; set; }

	/// <summary>
	/// Rotate the target using the rotation of points, otherwise use the curvature of the path.
	/// </summary>
	[Property]
	public bool UsePointRotation { get; set; }

	/// <summary>
	/// Time is controlled manually.
	/// </summary>
	[Property]
	public bool Manual { get; set; }

	/// <summary>
	/// How long the path takes to complete.
	/// </summary>
	[Property, ShowIf( nameof( Manual ), false )]
	public float Duration { get; set; } = 10;

	public enum SplineType
	{
		Tcb,
		CatmullRom
	};

	[Title( "Mode" ), Group( "Spline" )]
	[Property] public SplineType SplineMode { get; set; }

	[Title( "Tension" ), Group( "Spline" ), ShowIf( nameof( SplineMode ), SplineType.Tcb )]
	[Property, Range( -1, 1 )] public float SplineTension { get; set; }

	[Title( "Continuity" ), Group( "Spline" ), ShowIf( nameof( SplineMode ), SplineType.Tcb )]
	[Property, Range( -1, 1 )] public float SplineContinuity { get; set; }

	[Title( "Bias" ), Group( "Spline" ), ShowIf( nameof( SplineMode ), SplineType.Tcb )]
	[Property, Range( -1, 1 )] public float SplineBias { get; set; }

	private Vector3[] _previewPoints;

	protected override void OnStart()
	{
		base.OnStart();

		if ( Scene.IsEditor )
		{
			if ( !GameObject.GetAllObjects( true )
				.Select( x => x.Components.Get<MotionPathPoint>() )
				.Where( x => x.IsValid() )
				.Any() )
			{
				var go = new GameObject( true, "Point" );
				go.SetParent( GameObject, false );
				go.Components.Create<MotionPathPoint>( true );
			}
		}
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( _previewPoints is null )
			return;

		using ( Gizmo.Hitbox.LineScope() )
		{
			for ( var i = 0; i < _previewPoints.Length - 1; i++ )
			{
				var pointA = Transform.World.PointToLocal( _previewPoints[i] );
				var pointB = Transform.World.PointToLocal( _previewPoints[i + 1] );
				Gizmo.Draw.Color = Gizmo.IsSelected ? Gizmo.Colors.Active : Gizmo.IsHovered ? Gizmo.Colors.Hovered : Color.White.WithAlpha( 0.5f );
				Gizmo.Draw.LineThickness = Gizmo.IsHovered || Gizmo.IsSelected ? 2 : 1;
				Gizmo.Draw.Line( pointA, pointB );
			}
		}

		if ( Target.IsValid() )
		{
			Gizmo.Draw.Color = Gizmo.Colors.Forward;
			var pointA = Transform.World.PointToLocal( Target.WorldPosition );
			var pointB = Transform.World.NormalToLocal( Target.WorldRotation.Forward ) * 5;
			Gizmo.Draw.SolidCone( pointA, pointB, 2 );
		}
	}

	private Vector3 GetPointOnSpline( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float delta )
	{
		if ( SplineMode == SplineType.Tcb )
			return Vector3.TcbSpline( p0, p1, p2, p3, SplineTension, SplineContinuity, SplineBias, delta );
		else if ( SplineMode == SplineType.CatmullRom )
			return Vector3.CatmullRomSpline( p0, p1, p2, p3, delta );

		return default;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		var motionPoints = GameObject.GetAllObjects( true )
			.Select( x => x.Components.Get<MotionPathPoint>() )
			.Where( x => x.IsValid() )
			.ToArray();

		var points = motionPoints.Select( x => x.Transform.Position )
			.ToArray();

		if ( SplineMode == SplineType.Tcb )
			_previewPoints = points.TcbSpline( 32, SplineTension, SplineContinuity, SplineBias ).ToArray();
		else if ( SplineMode == SplineType.CatmullRom )
			_previewPoints = points.CatmullRomSpline( 32 ).ToArray();

		if ( points.Length < 2 )
			return;

		if ( points.Length == 2 )
			_previewPoints = new Vector3[2] { points[0], points[1] };

		if ( Target.IsValid() )
		{
			var pointCount = points.Length;

			if ( !Manual && !Duration.AlmostEqual( 0.0f ) )
				Time = Sandbox.Time.Now % Duration / Duration;

			var time = Time.Clamp( 0.0f, 1.0f );

			var segmentIndex = (int)((pointCount - 1) * time);
			segmentIndex = Math.Min( segmentIndex, pointCount - 2 );
			var delta = ((pointCount - 1) * time) - segmentIndex;

			var p0 = segmentIndex > 0 ? points[segmentIndex - 1] : points[0];
			var p1 = points[segmentIndex];
			var p2 = segmentIndex < pointCount - 1 ? points[segmentIndex + 1] : points[pointCount - 1];
			var p3 = segmentIndex < pointCount - 2 ? points[segmentIndex + 2] : points[pointCount - 1];

			var offset = time.AlmostEqual( 1.0f ) ? -0.01f : 0.01f;
			var positionA = GetPointOnSpline( p0, p1, p2, p3, delta );

			if ( UsePointRotation )
			{
				var r1 = motionPoints[segmentIndex];
				var r2 = segmentIndex < pointCount - 1 ? motionPoints[segmentIndex + 1] : motionPoints[pointCount - 1];
				var rotationStart = r1.Transform.Rotation;
				var rotationEnd = r2.Transform.Rotation;

				if ( r1.LookAt.IsValid() )
					rotationStart = Rotation.LookAt( (r1.LookAt.Transform.Position - r1.Transform.Position).Normal );

				if ( r2.LookAt.IsValid() )
					rotationEnd = Rotation.LookAt( (r2.LookAt.Transform.Position - r2.Transform.Position).Normal );

				Target.Transform.Rotation = Rotation.Slerp( rotationStart, rotationEnd, delta );
			}
			else
			{
				var positionB = GetPointOnSpline( p0, p1, p2, p3, delta + offset );
				var forward = (positionB - positionA).Normal * MathF.Sign( offset );
				var right = forward.Cross( Vector3.Up ).Normal;
				var up = right.Cross( forward ).Normal;

				Target.Transform.Rotation = Rotation.LookAt( forward, up );
			}

			Target.Transform.Position = positionA;
		}
	}
}

public class MotionPathPoint : Component, Component.ExecuteInEditor
{
	/// <summary>
	/// Look at this object.
	/// </summary>
	[Property]
	public GameObject LookAt { get; set; }

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		const float radius = 2.0f;
		Gizmo.Hitbox.DepthBias = 0.1f;
		Gizmo.Hitbox.Sphere( new Sphere( 0, radius ) );
		Gizmo.Draw.Color = Gizmo.IsSelected ? Gizmo.Colors.Active : Gizmo.IsHovered ? Gizmo.Colors.Hovered : Color.White;
		Gizmo.Draw.SolidSphere( 0, radius );

		if ( Gizmo.IsSelected || Gizmo.IsHovered )
		{
			var end = LookAt.IsValid() ? Transform.World.PointToLocal( LookAt.Transform.Position ) : Vector3.Forward * 6;
			Gizmo.Draw.Color = Gizmo.Colors.Forward;
			Gizmo.Draw.Arrow( 0, end, 2, 1 );
		}
	}
}
