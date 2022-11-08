using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Transform buttonScale;
    [Header("Audio Clip")]
    AudioSource audioSource;
    [SerializeField]
    private AudioClip audioClip;
    Vector3 defaultScale;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        defaultScale = buttonScale.localScale;
    }
    void Update()
    {
    }
    public void OnClickNewGame()
    {
        Debug.Log("게임시작");
        SceneManager.LoadScene("StartGame");
        audioSource.GetComponent<AudioSource>();
        audioSource.Play();
    }
    public void OnClickPractice()
    {
        Debug.Log("훈련장");
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnClickOption()
    {
        Debug.Log("옵션");
    }
    public void OnClickQuit()
    {
        Debug.Log("종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
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