using System.Collections.Generic;
using model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Gemstones
{
    public class InlayGemstones : MonoBehaviour
    {
        private List<Gemstone> _gemstones;
        [SerializeField] private List<Image> images;
        [SerializeField] private List<TextMeshProUGUI> descriptions;
        
        
        public void Init(List<Gemstone> gemstones)
        {
            _gemstones = gemstones;
            for (var i = 0; i < images.Count; i++)
            {
                if (i < gemstones.Count)
                {
                    images[i].sprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstones[i].Level:D2}");
                    descriptions[i].text = gemstones[i].Description;
                }
                else
                {
                    images[i].sprite = null;
                    descriptions[i].text = "";
                }
            }
        }
    }
}