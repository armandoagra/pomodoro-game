using UnityEngine;
using UnityEngine.EventSystems;

public class Pomodoro : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
{
    [SerializeField] private int workDuration = 60 * 25;
    [SerializeField] private int restDuration = 60 * 5;
    [SerializeField] private int bigRestDuration = 60 * 15;
    [SerializeField] private int maxSessions = 4;
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    private int sessions = 0;
    private float timer = 0f;
    private State currentState = State.Work;
    private float currentMaxTimer = 0f;
    private bool isPaused = true;
    private float uiTimer = 0f;
    [SerializeField] private Color workColor;
    [SerializeField] private Color restColor;
    //[SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private UnityEngine.UI.Image image;
    private Camera cam;
    [SerializeField] private bool isOnTop = false, isDragging = false;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip workEndSfx, restEndSfx;
    private float pointDownTime;
    private RectTransform rectTransform;
    private enum State
    {
        Work, Rest, BigRest
    }

    private void Awake()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        //spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        SnapToScreenEdge();
        currentMaxTimer = workDuration;
        Application.targetFrameRate = 5;
    }


    void Update()
    {
        if (!isPaused)
        {
            uiTimer += Time.deltaTime;
            timer += Time.deltaTime;
            if (timer > currentMaxTimer)
            {
                ChangeState();
            }
            if (uiTimer > 1f)
            {
                uiTimer = 0f;
                UpdateUI();
            }
        }
        if (isOnTop)
        {
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
            {
                transform.localScale *= 1.1f;
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                transform.localScale *= 0.9f;
            }
        }
        if (isDragging && pointDownTime < Time.time - 0.15f)
        {
            Vector3 mousePos = cam.WorldToViewportPoint(Input.mousePosition);
            Vector3 newPosition = cam.ViewportToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            transform.position = newPosition;
        }

    }
    public void BtnTogglePause()
    {
        isPaused = !isPaused;
    }

    private void ChangeState()
    {
        timer = 0f;
        switch (currentState)
        {
            case State.Work:
                sessions++;
                image.color = restColor;
                sfxSource.PlayOneShot(workEndSfx);
                if (sessions == maxSessions)
                {
                    currentMaxTimer = bigRestDuration;
                    currentState = State.BigRest;
                    sessions = 0;
                }
                else
                {
                    currentMaxTimer = restDuration;
                    currentState = State.Rest;
                }
                break;
            case State.Rest:
                sfxSource.PlayOneShot(restEndSfx);
                currentState = State.Work;
                image.color = workColor;
                currentMaxTimer = workDuration;
                break;
            case State.BigRest:
                sfxSource.PlayOneShot(restEndSfx);
                currentState = State.Work;
                image.color = workColor;
                currentMaxTimer = workDuration;
                break;
        }
        UpdateUI();
        isPaused = true;
    }

    private void UpdateUI()
    {
        float timeLeft = currentMaxTimer - timer;
        int minutes = (int)(timeLeft / 60);
        int seconds = (int)(timeLeft % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void SnapToScreenEdge()
    {
        //Vector3 newPosition = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        //transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
        rectTransform.anchoredPosition = new Vector2(100, 100);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isOnTop = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isOnTop = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        pointDownTime = Time.time;
        Application.targetFrameRate = 60;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        Application.targetFrameRate = 5;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isPaused = !isPaused;
        UpdateUI();
    }
}
