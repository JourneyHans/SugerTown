using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour 
{
    public Button StartBtn;

	void Start () 
    {
        StartBtn.onClick.AddListener(ClickStart);
	}

    void ClickStart()
    {
        SceneManager.LoadScene("GameScene");
    }
}
