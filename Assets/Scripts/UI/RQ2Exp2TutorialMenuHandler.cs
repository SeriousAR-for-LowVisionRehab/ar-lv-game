using UnityEngine;
using UnityEngine.Video;

public class RQ2Exp2TutorialMenuHandler : MonoBehaviour
{
    private int _indexPressPrefab = 0;
    private int _indexPinchSlidePrefab = 1;
    private int _indexBouncyBall = 2;

    [SerializeField] private VideoPlayer _gesturePressVideo;
    [SerializeField] private VideoPlayer _gesturePinchSlide;

    /// <summary>
    /// Show object to learn and practice the "press" gesture
    /// </summary>
    public void GesturePressTutorial()
    {
        GameManager.Instance.AvailableTutorialPrefabs[_indexPressPrefab].SetActive(true);
        _gesturePressVideo.gameObject.SetActive(true);
        _gesturePressVideo.Play();
    }

    /// <summary>
    /// Show object to learn and practice the "pinch & slide" gesture
    /// </summary>
    public void GesturePinchSlideTutorial()
    {
        GameManager.Instance.AvailableTutorialPrefabs[_indexPinchSlidePrefab].SetActive(true);
        _gesturePinchSlide.gameObject.SetActive(true);
        _gesturePinchSlide.Play();
    }

    public void InstantiateBouncyBall()
    {
        GameObject bouncyBall = Instantiate(GameManager.Instance.AvailableTutorialPrefabs[_indexBouncyBall], this.transform);
        Destroy(bouncyBall, 2.0f);
    }

    /// <summary>
    /// Move back to Home Menu
    /// </summary>
    public void EndTutorial()
    {
        foreach(var tutoPuzzle in GameManager.Instance.AvailableTutorialPrefabs)
        {
            tutoPuzzle.SetActive(false);
        }

        _gesturePressVideo.Stop();
        _gesturePressVideo.gameObject.SetActive(false);
        _gesturePinchSlide.Stop();
        _gesturePinchSlide.gameObject.SetActive(false);

        GameManager.Instance.SwitchToHomeMenu();
    }
}
