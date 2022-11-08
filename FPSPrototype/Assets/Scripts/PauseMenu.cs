using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject go_BaseUi;

    public Transform buttonScale;
    Vector3 defaultScale;
    private void Start()
    {
        defaultScale = buttonScale.localScale;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!PlayerController.isPause)
                CallMenu();
            else
                CloseMenu();
        }
    }
    public void CallMenu()
    {
        PlayerController.isPause = true;
        go_BaseUi.SetActive(true);
        Time.timeScale = 0f;
    }
    private void CloseMenu()
    {
        PlayerController.isPause = false;
        go_BaseUi.SetActive(false);
        Time.timeScale = 1f;
    }
    public void ClickResume()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.isPause = false;
        go_BaseUi.SetActive(false);
        Time.timeScale = 1f;
    }
    public void ClickQuit()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void ClickExit()
    {
        Application.Quit();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonScale.localScale = defaultScale * 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonScale.localScale = defaultScale;
    }
}
