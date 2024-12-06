using UnityEngine;
using Avata1;
using Avata2;
using Avata3;
using Avata4;

public class AvatarManager : MonoBehaviour
{
    public GameObject gameManagerCanvas; // 게임매니저 캔버스
    public GameObject backToMainCanvas;
    public GameObject[] avatarCanvases; // 각 아바타의 고유 캔버스
    public GameObject[] avatars; // 아바타 배열
    public CustomCameraController customCameraController; // 카메라 컨트롤러
    private int activeAvatarIndex = -1; // 현재 활성화된 아바타


    public string[] characterIds = { "펭리자베스 14세", "손육공", "마루", "군인" };


    void Awake()
    {
        Debug.Log("AvatarManager Awake started");

        // GameManagerCanvas를 제외한 모든 자식 오브젝트를 Avatar로 간주
        int avatarCount = transform.childCount - 2;
        Debug.Log($"Found {avatarCount} avatars under AvatarManager");

        avatarCanvases = new GameObject[avatarCount];
        avatars = new GameObject[avatarCount];

        for (int i = 0; i < avatarCount; i++)
        {
            // Avatar 가져오기
            var avatar = transform.GetChild(i + 1).gameObject;
            Debug.Log($"Processing avatar: {avatar.name}");
            avatars[i] = avatar;

            // Avatar의 Canvas 찾기
            var canvas = avatar.transform.Find("AvataCanvas");
            if (canvas == null)
            {
                Debug.LogError($"AvataCanvas not found under {avatar.name}");
                continue;
            }
            Debug.Log($"AvataCanvas found under {avatar.name}");
            avatarCanvases[i] = canvas.gameObject;
        }
         // BackToMainCanvas는 마지막 자식으로 가정
        backToMainCanvas = transform.GetChild(transform.childCount - 1).gameObject;
    
    }
    

    void Start()
    {
        // 초기 상태: GameManagerCanvas 활성화, 모든 아바타 및 캔버스 비활성화
        gameManagerCanvas.SetActive(true);
        backToMainCanvas.SetActive(false);

        foreach (var canvas in avatarCanvases)
        {
            if (canvas != null) canvas.SetActive(false);
        }

        foreach (var avatar in avatars)
        {
            if (avatar != null) avatar.SetActive(false);
        }
    }

    public void SelectAvatar(int index)
    {
        if (index < 0 || index >= avatars.Length)
        {
            Debug.LogError($"Invalid index {index}. Ensure it's between 0 and {avatars.Length - 1}");
            return;
        }

        // 이전 아바타 비활성화
        if (activeAvatarIndex != -1)
        {
            var previousModel = avatars[activeAvatarIndex].transform.Find("Model");
            if (previousModel != null)
            {
                // 이전 모델의 네임스페이스에 따라 비활성화
                switch (activeAvatarIndex)
                {
                    case 0:
                        previousModel.GetComponent<Avata1.CharacterMovement>().isActive = false;
                        break;
                    case 1:
                        previousModel.GetComponent<Avata2.CharacterMovement>().isActive = false;
                        break;
                    case 2:
                        previousModel.GetComponent<Avata3.CharacterMovement>().isActive = false;
                        break;
                    case 3:
                        previousModel.GetComponent<Avata4.CharacterMovement>().isActive = false;
                        break;
        
                    default:
                        Debug.LogError("Invalid activeAvatarIndex for disabling");
                        break;
                }
            }

            avatars[activeAvatarIndex].SetActive(false);
            avatarCanvases[activeAvatarIndex].SetActive(false);
        }

        // 선택된 아바타 활성화
        activeAvatarIndex = index;
        avatars[activeAvatarIndex].SetActive(true);
        avatarCanvases[activeAvatarIndex].SetActive(true);

        // 선택된 Model 활성화
        var currentModel = avatars[index].transform.Find("Model");
        if (currentModel == null)
        {
            Debug.LogError($"Model not found under {avatars[index].name}");
            return;
        }

        // 선택된 모델의 네임스페이스에 따라 활성화
        switch (index)
        {
            case 0:
                currentModel.GetComponent<Avata1.CharacterMovement>().isActive = true;
                break;
            case 1:
                currentModel.GetComponent<Avata2.CharacterMovement>().isActive = true;
                break;
            case 2:
                currentModel.GetComponent<Avata3.CharacterMovement>().isActive = true;
                break;
            case 3:
                currentModel.GetComponent<Avata4.CharacterMovement>().isActive = true;
                break;
            default:
                Debug.LogError("Invalid index for enabling");
                break;
        }

        // 카메라 타겟을 Model로 설정
        customCameraController.SetTarget(currentModel);

        // 게임매니저 캔버스 비활성화
        gameManagerCanvas.SetActive(false);

        backToMainCanvas.SetActive(true); // BackToMainCanvas 활성화
    }



    public void BackToMainCanvas()
    {
        // 모든 아바타와 캔버스 비활성화
        foreach (var avatar in avatars)
        {
            if (avatar != null) avatar.SetActive(false);
        }

        foreach (var canvas in avatarCanvases)
        {
            if (canvas != null) canvas.SetActive(false);
        }

        // 활성화된 Model 비활성화
        if (activeAvatarIndex != -1)
        {
            var currentModel = avatars[activeAvatarIndex].transform.Find("Model");
            if (currentModel != null)
            {
                // 활성화된 아바타의 네임스페이스에 따라 비활성화
                switch (activeAvatarIndex)
                {
                    case 0:
                        currentModel.GetComponent<Avata1.CharacterMovement>().isActive = false;
                        break;
                    case 1:
                        currentModel.GetComponent<Avata2.CharacterMovement>().isActive = false;
                        break;
                    case 2:
                        currentModel.GetComponent<Avata3.CharacterMovement>().isActive = false;
                        break;
                    case 3:
                        currentModel.GetComponent<Avata4.CharacterMovement>().isActive = false;
                        break;
                    default:
                        Debug.LogError("Invalid activeAvatarIndex for disabling");
                        break;
                }
            }

            activeAvatarIndex = -1;
        }

        // 게임매니저 캔버스 활성화
        gameManagerCanvas.SetActive(true);
        backToMainCanvas.SetActive(false); // BackToMainCanvas 비활성화
    }


    public void OpenChatOverlay()
    {


        if (activeAvatarIndex == -1)
        {
            Debug.LogError("No active avatar selected!");
            return;
        }

        backToMainCanvas.SetActive(false);

        avatarCanvases[activeAvatarIndex].SetActive(false);

        // 현재 활성화된 아바타의 isActive를 비활성화
        var currentModel = avatars[activeAvatarIndex].transform.Find("Model");
        if (currentModel != null)
        {
            switch (activeAvatarIndex)
            {
                case 0:
                    currentModel.GetComponent<Avata1.CharacterMovement>().isActive = false;
                    break;
                case 1:
                    currentModel.GetComponent<Avata2.CharacterMovement>().isActive = false;
                    break;
                case 2:
                    currentModel.GetComponent<Avata3.CharacterMovement>().isActive = false;
                    break;
                case 3:
                    currentModel.GetComponent<Avata4.CharacterMovement>().isActive = false;
                    break;
                default:
                    Debug.LogError("Invalid activeAvatarIndex for disabling");
                    break;
            }
        }

/*
        // ChatManager에 캐릭터 ID 전달
        var chatManager = chatOverlayCanvas.GetComponent<ChatManager>();
        if (chatManager != null)
        {
            chatManager.SetCharacterId(characterIds[activeAvatarIndex]);
        }
        else
        {
            Debug.LogError("ChatManager is not attached to ChatOverlayCanvas!");
        }
        */
        
    }

    public void CloseChatOverlay()
    {

        avatarCanvases[activeAvatarIndex].SetActive(true);
        backToMainCanvas.SetActive(true);

        // 현재 활성화된 아바타의 isActive를 다시 활성화
        var currentModel = avatars[activeAvatarIndex].transform.Find("Model");
        if (currentModel != null)
        {
            switch (activeAvatarIndex)
            {
                case 0:
                    currentModel.GetComponent<Avata1.CharacterMovement>().isActive = true;
                    break;
                case 1:
                    currentModel.GetComponent<Avata2.CharacterMovement>().isActive = true;
                    break;
                case 2:
                    currentModel.GetComponent<Avata3.CharacterMovement>().isActive = true;
                    break;
                case 3:
                    currentModel.GetComponent<Avata4.CharacterMovement>().isActive = true;
                    break;
                default:
                    Debug.LogError("Invalid activeAvatarIndex for enabling");
                    break;
            }
        }
    }

}
