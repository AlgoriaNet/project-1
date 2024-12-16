using System.Collections.Generic;
using entity;
using UnityEngine;
using UnityEngine.UI;

namespace battle
{
    public class DamagePresentationManager : MonoBehaviour
    {
        private class DamagePresentation
        {
            public Color Color { get; set; }
            public int FontSize { get; set; } = 250;
            public FontStyle FontStyle { get; set; } = FontStyle.Bold;
            public string Icon { get; set; }

            public DamagePresentation(Color color)
            {
                Color = color;
            }
        }

        public static DamagePresentationManager Instance;
        private readonly Dictionary<DamageType, DamagePresentation> _damagePresentations = new();
        public GameObject UItexte;

        private void Awake()
        {
            Instance = this;
            _damagePresentations[DamageType.Cure] = new DamagePresentation(new Color(1f, 1f, 1f));
            _damagePresentations[DamageType.Fire] = new DamagePresentation(new Color(1f, 1f, 1f));
            _damagePresentations[DamageType.Ice] = new DamagePresentation(new Color(0f, 1f, 1f));
            _damagePresentations[DamageType.Light] = new DamagePresentation(new Color(1f, 1f, 1f));
            _damagePresentations[DamageType.Wind] = new DamagePresentation(new Color(1f, 1f, 1f));
            _damagePresentations[DamageType.Mechanical] = new DamagePresentation(new Color(1f, 1f, 1f));
            _damagePresentations[DamageType.Physics] = new DamagePresentation(new Color(1f, 1f, 1f));
            _damagePresentations[DamageType.Burn] = new DamagePresentation(Color.red);
        }

        public void ShowDamage(DamageResult damageResult, Transform transform)
        {
            if (_damagePresentations.ContainsKey(damageResult.DamageType))
            {
                var ui = Instantiate(UItexte);
                Destroy(ui, 2);
                var a = ui.GetComponentInChildren<Text>();
                a.transform.position = transform.position;
                a.text = damageResult.Damage.ToString();
                a.color = _damagePresentations[damageResult.DamageType].Color;
                a.fontSize = _damagePresentations[damageResult.DamageType].FontSize;
                a.fontStyle = _damagePresentations[damageResult.DamageType].FontStyle;
                if (damageResult.IsCrit)
                {
                    a.color = Color.yellow;
                    a.fontStyle = FontStyle.Bold;
                    a.fontSize = 300;
                }
            }
        }
    }
}