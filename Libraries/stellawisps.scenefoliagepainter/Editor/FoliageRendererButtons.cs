using Foliage;

namespace Editor.FoliagePainter;

	using Editor;
using Sandbox;
	using System;
using System.IO;
using System.Linq;


[Sandbox.CustomEditor( typeof( FoliageRenderer.OptionsWidget ) )]
public class FoliageRendererButtons : ControlWidget
{
	FoliageRenderer FoliageManager { get; set; }
	
	public FoliageRendererButtons( Sandbox.SerializedProperty property ) : base( property )
    {
        FoliageManager = property.Parent.Targets.First() as FoliageRenderer;

        Layout = Layout.Column();
        Layout.Spacing = 2;

        Rebuild();
    }

    [EditorEvent.Hotload]
    void Rebuild()
    {
        Layout.Clear( true );

        {
            var button = new Button( this );
            button.Text = "Delete All Foliage";
            button.ToolTip = "Removes all foliage from this renderer";
            button.Clicked += ResetResource;
            Layout.Add( button );
        }
        
        {
	        var button = new Button( this );
	        button.Text = "Update Foliage";
	        button.ToolTip = "Update all foliage renderers if a model was changed in a .fol asset";
	        button.Clicked += UpdateResource;
	        Layout.Add( button );
        }

    }

    void ResetResource()
    {
	    FoliageManager.ClearAll();
        Rebuild();
    }

    void UpdateResource()
    {
	    FoliageManager.UpdateRenderers();
    }
    
}

