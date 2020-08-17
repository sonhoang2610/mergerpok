using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using static AssetUsageFinder.PrefabUtilities;
using Object = UnityEngine.Object;

namespace AssetUsageFinder {
	[Serializable]
	class SearchTarget {
		public Object Target;
		public Option<Object[]> Nested;
		public Object Root;
		public Scene Scene;
		public PrefabStage Stage;

		public SearchTarget(Object target, FindModeEnum findMode, string sceneOrStagePath = null) {
			Asr.IsNotNull(target, "Asset you're trying to search is corrupted");
			Target = target;
			var path = sceneOrStagePath ?? AssetDatabase.GetAssetPath(Target);
			switch (findMode) {
				case FindModeEnum.File:
					Asr.IsTrue(string.IsNullOrEmpty(sceneOrStagePath));
					Root = AssetDatabase.LoadMainAssetAtPath(path);
					Nested = AufUtils.LoadAllAssetsAtPath(path);
					break;
				case FindModeEnum.Scene:
				case FindModeEnum.Stage:
					Root = Target;
					if (Target is GameObject go) {
						Nested = go.GetComponents<Component>().OfType<Object>().ToArray();
						Stage = PrefabStageUtility.GetCurrentPrefabStage();
						if (findMode == FindModeEnum.Scene) {
							if (string.IsNullOrEmpty(sceneOrStagePath))
								sceneOrStagePath = go.scene.path;
							Scene = SceneManager.GetSceneByPath(sceneOrStagePath);
						}
					}
					else if (Target is Component c) {
						Nested = default;
						if (findMode == FindModeEnum.Scene) {
							if (string.IsNullOrEmpty(sceneOrStagePath))
								sceneOrStagePath = c.gameObject.scene.path;
							Scene = SceneManager.GetSceneByPath(sceneOrStagePath);
						}
					}
					else {
						Nested = default;
					}
					break;
			}
		}

		public bool Check(Object t) {
			var tt = Target == t;
			return t && Nested.TryGet(out var n) && n.Aggregate(tt, (current, o) => current || o == t);
		}
	}
}