using UnityEngine;

public class SceneName : MonoBehaviour
{
    [Header("Nama Scene yang Akan Dimuat")]
    public string namaScene;

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(namaScene))
        {
            if (SceneController.instance != null)
            {
                SceneController.instance.LoadScene(namaScene);
            }
            else
            {
                Debug.LogWarning("SceneController instance tidak ditemukan!");
            }
        }
        else
        {
            Debug.LogWarning("Nama scene belum diisi!");
        }
    }
}
