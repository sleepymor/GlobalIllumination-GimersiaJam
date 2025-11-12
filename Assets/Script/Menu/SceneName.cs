using UnityEngine;

public class SceneName : MonoBehaviour
{
    [Header("Nama Scene yang Akan Dimuat")]
    public string namaScene;

    [Header("Tag yang Bisa Memicu Trigger")]
    public string triggerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            LoadScene();
        }
    }

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
