using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUIManager : MonoBehaviour
{
    // UI 元素绑定
    public TMP_InputField emailInputField;    // 邮箱输入框
    public TMP_InputField passwordInputField; // 密码输入框
    public Button loginButton;            // 登录按钮
    public Text statusMessage;            // 状态提示

    private AuthService authService = new AuthService(); // 创建 AuthService 实例

    void Start()
    {
        Time.timeScale = 1.0f;
        // 绑定登录按钮点击事件
        loginButton.onClick.AddListener(() => Login());
    }

    public void Login() 
    {
        
        string email = emailInputField.text;
        string encryptPassword = EncryptionUtil.Encrypt(passwordInputField.text);

        // 检查输入合法性
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(encryptPassword))
        {
            UpdateStatus("Email and Password cannot be empty!", Color.red);
            Debug.Log("Email and Password cannot be empty!");
            return;
        }

        // 调用登录服务
        StartCoroutine(authService.Login(
            email, 
            encryptPassword, 
            onSuccess: OnLoginSuccess, 
            onError: OnLoginError
        ));
    }

    private void OnLoginSuccess(AuthService.LoginResponse response)
    {
        // 存储 Token 和 user_id
        PlayerPrefs.SetString("authToken", response.token);
        PlayerPrefs.SetInt("user_id", response.user.id);
        WebSocketManager.Instance.ConnectWebSocket();
        

        // 登录成功后执行其他操作
        Debug.Log($"User ID: {response.user.id}, Token: {response.token}");
        // SceneManager.LoadScene("MainScene"); // 示例场景切换代码
    }

    private void OnLoginError(string errorMessage)
    {
        UpdateStatus(errorMessage, Color.red);
    }

    private void UpdateStatus(string message, Color color)
    {
        statusMessage.text = message;
        statusMessage.color = color;
    }
}