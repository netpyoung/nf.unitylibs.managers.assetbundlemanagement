using NF.UnityLibs.Managers.AssetBundleManagement;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HelloWorld
{
	public class NewMonoBehaviourScript : MonoBehaviour
	{
		[SerializeField]
		AssetBundleManager _bundle = new AssetBundleManager();

		async void Start()
		{
			//string deviceBaseDirFpath = AssetBundleManager_ForEditorBuild0.GetDeviceBaseDirFpath_ForEditorBuild0();
			//string manifestName = AssetBundleManager_ForEditorBuild0.MANIFEST_NAME;
			string deviceBaseDirFpath = string.Empty;
			string manifestName = string.Empty;

			Exception? exOrNull = await _bundle.InitAsync(deviceBaseDirFpath, manifestName);
			if (exOrNull is Exception ex)
			{
				Debug.LogException(ex);
				return;
			}
			TaskBundle<GameObject> task = _bundle.RentBundleOrNull<GameObject>("cube")!;
			while (!task.IsCompleted)
			{
				await Task.Yield();
			}

			Bundle<GameObject> bundle = await task;
			
			Instantiate(bundle.AssetOrNull, Vector3.zero, Quaternion.identity);

			await Task.Delay(3 * 1000);

			await _bundle.ReturnBundleAsync(bundle);
		}

		private void Update()
		{
			_bundle.Update();
		}

		private void OnDestroy()
		{
			_bundle.Dispose();
		}
	}
}