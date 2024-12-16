using UnityEngine;
using UnityEngine.UI;
using Path = utils.Path;
using Random = UnityEngine.Random;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;
    
    [SerializeField] private Image loadingImage;
    [SerializeField] private float speed = 1f;
    [SerializeField] private int maxIndex;

    private Vector2 _gameObjectSize;
    private float _width;
    private float _height;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        var rectTransform = gameObject.GetComponentInParent<RectTransform>();
        _gameObjectSize = rectTransform.rect.size;
        _width = _gameObjectSize.x;
        _height = _gameObjectSize.y;
    }


    private void OnEnable()
    {
        Debug.Log("LoadingManager OnEnable");
        SetImage();
    }

    private void SetImage()
    {
        var loadingIndex = Random.Range(1, maxIndex + 1);
        var image = Resources.Load<Sprite>(Path.GetPath(Path.LoadingImage, loadingIndex.ToString()));
        Debug.Log("Load path: " + Path.GetPath(Path.LoadingImage, loadingIndex.ToString()));
        Debug.Log("Load image: " + image);
        loadingImage.sprite = image;
        var imageProperWidth = image.rect.width;
        var imageProperHeight = image.rect.height;
        Debug.Log(imageProperWidth + " " + imageProperHeight);
        Debug.Log(_width + " " + _height);
        var offsetX = (imageProperWidth * (imageProperHeight / _height) - _width) / _width;
        var imageRectTransform = loadingImage.GetComponent<RectTransform>();
        imageRectTransform.anchorMin = new Vector2(offsetX, 0);
        imageRectTransform.anchorMax = new Vector2(1 - offsetX, 1);
    }
}