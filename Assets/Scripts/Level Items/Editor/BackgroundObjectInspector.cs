﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ClipperLib;
using Poly2Tri;

[CustomEditor(typeof(BackgroundObjectController))]
public class BackgroundObjectInspector : Editor {

	public enum Type { PLANE, SHAPE }

	private BackgroundObjectController _editorTarget = null;
	private BackgroundObjectController editorTarget {
		get {
			if (_editorTarget == null) {
				_editorTarget = target as BackgroundObjectController;
			}
			return _editorTarget;
		}
	}

	private const float clipperScale = 10000f;

	private Type type = Type.PLANE;

	// Plane variables
	private Rect _planeOutline = new Rect( -5f, -5f, 10f, 10f );
	// Shape variables
	public Shape2D shapeOutline;


	public override void OnInspectorGUI() {
		Vector2 uvAnimation = EditorGUILayout.Vector2Field( "UV Animation:", editorTarget.animationSpeed );
		Material meshMaterial = EditorGUILayout.ObjectField( editorTarget.meshRenderer.sharedMaterial, typeof(Material), false ) as Material;
		GUILayout.Space( 15f );
		GUILayout.Label( "MESH" );
		Vector2 tileSize = EditorGUILayout.Vector2Field( "Tile Size:", editorTarget.tileSize );

		type = (Type)EditorGUILayout.EnumPopup( "Type", type );

		if (GUI.changed) {
			editorTarget.animationSpeed = uvAnimation;
			editorTarget.meshRenderer.sharedMaterial = meshMaterial;
			editorTarget.tileSize = tileSize;
		}

		if (GUILayout.Button( "Generate Shape Mesh" )) {
			if ( type == Type.PLANE ) {
				editorTarget.meshFilter.mesh = MeshGenerator.GeneratePlaneMesh( editorTarget.tileSize, editorTarget.planeOutline, shapeOutline );
			} else {
				editorTarget.meshFilter.mesh = MeshGenerator.GenerateShapeMesh( editorTarget.tileSize, shapeOutline );
			}
		}
	}

	void OnEnable() {
		if( editorTarget.type != 0 ) { type = Type.SHAPE; } else { type = Type.PLANE; }

		if ( string.IsNullOrEmpty(editorTarget.shapeData) || editorTarget.shapeData == "null" ) { // No data was found so we create a new empty shape object
			shapeOutline = Shape2D.triangle;
		} else {
			JSONObject shapeObj = new JSONObject(editorTarget.shapeData);

			shapeOutline = new Shape2D();
			foreach (JSONObject vecObj in shapeObj.list) {
				Vector2 point = new Vector2( (float)vecObj.GetField("x").n, (float)vecObj.GetField("y").n );
				shapeOutline.AddPoint(point);
			}
		}
	}

	void OnDestroy() { // Serialize the shape
		JSONObject shapeObj = new JSONObject(JSONObject.Type.ARRAY);
		Vector2[] shapePoints = shapeOutline.GetPoints();
		
		foreach (Vector2 vec2 in shapePoints) {
			JSONObject vecObj = new JSONObject(JSONObject.Type.OBJECT);
			vecObj.AddField("x", vec2.x);
			vecObj.AddField("y", vec2.y);
			shapeObj.Add(vecObj);
		}

		string shapeObjStr = shapeObj.print();
		if (editorTarget.type != (int)type || _editorTarget.shapeData != shapeObjStr ) {
			editorTarget.type = (int)type;	
			editorTarget.shapeData = shapeObjStr;
			EditorUtility.SetDirty( editorTarget );
		}
	}

	void OnSceneGUI() {
		Transform targetTransform = editorTarget.transform;

		if ( type == Type.PLANE ) {

			Vector3[] outlinePoints = new Vector3[]{ new Vector3( editorTarget.planeOutline.xMin, 0f, editorTarget.planeOutline.yMin ),   // Bottom left
				new Vector3( editorTarget.planeOutline.xMax, 0f, editorTarget.planeOutline.yMin ),   // Bottom right
				new Vector3( editorTarget.planeOutline.xMax, 0f, editorTarget.planeOutline.yMax ),   // Top right
				new Vector3( editorTarget.planeOutline.xMin, 0f, editorTarget.planeOutline.yMax ),   // Top left
				new Vector3( editorTarget.planeOutline.xMin, 0f, editorTarget.planeOutline.yMin )};  // Bottom left

			for (int i=0; i < outlinePoints.Length; i++ ) { // Convert all points to world space
				outlinePoints[i] = editorTarget.transform.TransformPoint( outlinePoints[i] );
			}

			Handles.color = Color.blue;
			Handles.DrawAAPolyLine( 1f, outlinePoints );

			Handles.color = Color.white;
			Vector3 newPos;

			newPos = new Vector3( editorTarget.planeOutline.xMin, 0f, editorTarget.planeOutline.yMin );
			newPos = Handles.Slider2D( editorTarget.transform.TransformPoint( newPos ),
			                          targetTransform.up, targetTransform.forward, targetTransform.right, 0.2f * HandleUtility.GetHandleSize( newPos ) , Handles.CubeCap, 0f );
			newPos = editorTarget.transform.InverseTransformPoint( newPos );
			editorTarget.planeOutline.xMin = newPos.x;
			editorTarget.planeOutline.yMin = newPos.z;

			newPos = new Vector3( editorTarget.planeOutline.xMax, 0f, editorTarget.planeOutline.yMin );
			newPos = Handles.Slider2D( editorTarget.transform.TransformPoint( newPos ),
			                          targetTransform.up, targetTransform.forward, targetTransform.right, 0.2f * HandleUtility.GetHandleSize( newPos ), Handles.CubeCap, 0f );
			newPos = editorTarget.transform.InverseTransformPoint( newPos );
			editorTarget.planeOutline.xMax = newPos.x;
			editorTarget.planeOutline.yMin = newPos.z;

			newPos = new Vector3( editorTarget.planeOutline.xMax, 0f, editorTarget.planeOutline.yMax );
			newPos = Handles.Slider2D( editorTarget.transform.TransformPoint( newPos ),
			                          targetTransform.up, targetTransform.forward, targetTransform.right, 0.2f * HandleUtility.GetHandleSize( newPos ), Handles.CubeCap, 0f );
			newPos = editorTarget.transform.InverseTransformPoint( newPos );
			editorTarget.planeOutline.xMax = newPos.x;
			editorTarget.planeOutline.yMax = newPos.z;

			newPos = new Vector3( editorTarget.planeOutline.xMin, 0f, editorTarget.planeOutline.yMax );
			newPos = Handles.Slider2D( editorTarget.transform.TransformPoint( newPos ),
			                          targetTransform.up, targetTransform.forward, targetTransform.right, 0.2f * HandleUtility.GetHandleSize( newPos ), Handles.CubeCap, 0f );
			newPos = editorTarget.transform.InverseTransformPoint( newPos );
			editorTarget.planeOutline.xMin = newPos.x;
			editorTarget.planeOutline.yMax = newPos.z;

		}

		// Render lines and edit spheres for the shape

		Line2D[] outline = shapeOutline.GetOutline();

		for (int i=0; i<outline.Length; i++) {
			Line2D line = outline[i];
			Vector3 start = targetTransform.TransformPoint( VectorEx.Vec2ToVec3( line.start ) );
			Vector3 end = targetTransform.TransformPoint( VectorEx.Vec2ToVec3( line.end ) );
			Handles.color = Color.white;
			Handles.DrawLine(start, end);

			Handles.color = Color.green;
			Vector3 buttonPos = Vector3.Lerp(start, end, 0.5f);
			if (Handles.Button( buttonPos, targetTransform.rotation, 0.15f * HandleUtility.GetHandleSize( buttonPos ), 0.2f * HandleUtility.GetHandleSize( buttonPos ), Handles.SphereCap )) {
				shapeOutline.InsertPointAtIndex(VectorEx.Vec3ToVec2(targetTransform.InverseTransformPoint(buttonPos)), i+1);
			}
		}

		Vector2[] points = shapeOutline.GetPoints();
		for (int i=0; i<points.Length; i++) {
			Vector3 worldPos = targetTransform.TransformPoint(VectorEx.Vec2ToVec3(points[i]));

			Handles.color = Color.blue;
			Vector3 newPos = Handles.Slider2D( worldPos, targetTransform.up, targetTransform.forward, targetTransform.right, 0.25f * HandleUtility.GetHandleSize( worldPos ), Handles.SphereCap, 1f );
			Vector3 deleteButtonPos = worldPos + QuaternionEx.GetRightVector(SceneView.lastActiveSceneView.rotation) * 0.15f * HandleUtility.GetHandleSize( worldPos )
				+ QuaternionEx.GetUpVector(SceneView.lastActiveSceneView.rotation) * 0.15f * HandleUtility.GetHandleSize( worldPos );

			newPos = targetTransform.InverseTransformPoint(newPos);
			Vector2 newPos2D = VectorEx.Vec3ToVec2(newPos);

			if (!Mathf.Approximately( points[i].x, newPos2D.x) ||
			    !Mathf.Approximately( points[i].y, newPos2D.y) ) {
				shapeOutline.SetPointAtIndex(newPos2D, i);
			}

			Handles.color = Color.red;
			if (Handles.Button( deleteButtonPos, targetTransform.rotation, 0.05f * HandleUtility.GetHandleSize( deleteButtonPos ), 0.05f * HandleUtility.GetHandleSize( deleteButtonPos ), Handles.DotCap )) {
				shapeOutline.RemovePointAtIndex(i);
			}
		}


	}

}
