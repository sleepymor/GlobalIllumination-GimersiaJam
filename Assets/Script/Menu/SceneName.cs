using UnityEngine;
using UnityEngine.SceneManagement;


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
            // Simpan scene sekarang
            PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);

            // Simpan posisi player sebelum pindah scene
            PlayerPrefs.SetFloat("PlayerX", other.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", other.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", other.transform.position.z);
            PlayerPrefs.Save();

            // Pindah ke scene baru
            SceneController.instance.LoadScene(namaScene);
        }
    }

    // public void LoadScene()
    // {
    //     if (!string.IsNullOrEmpty(namaScene))
    //     {
    //         if (SceneController.instance != null)
    //         {
    //             PlayerPrefs.SetInt("SavedScene", SceneManager.GetActiveScene().buildIndex);
    //             PlayerPrefs.Save();
    //             SceneController.instance.LoadScene(namaScene);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("SceneController instance tidak ditemukan!");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Nama scene belum diisi!");
    //     }
    // }
}
