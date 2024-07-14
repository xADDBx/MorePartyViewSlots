using HarmonyLib;
using Kingmaker.UI;
using Kingmaker.UI.MVVM._PCView.Party;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System.Reflection;
using System.IO;

namespace MorePartyViewSlots {
    [HarmonyPatch(typeof(PartyPCView))]
    static class PartyPCView_Patches {
        private static Sprite hudBackground8;
        private static float itemWidth;
        private static float firstX;
        private static float scale;

        [HarmonyPatch("Initialize"), HarmonyPrefix]
        static void Initialize(PartyPCView __instance) {
            if (PartyVM_Patches.SupportedSlots == 6)
                return;

            try {


                Main.log.Log("INSTALLING BUBBLE GROUP PANEL");

                if (hudBackground8 == null) {
                    hudBackground8 = AssetLoader.LoadInternal("", "Background.png", new Vector2Int(1746, 298));
                }
                var vp = __instance.transform.Find("Viewport");
                var oldPos = vp.position;
                vp.position = new(oldPos.x - 2.5f, oldPos.y);
                __instance.transform.Find("Background").GetComponent<Image>().sprite = hudBackground8;

                scale = 6 / (float)PartyVM_Patches.WantedSlots;
                itemWidth = (__instance.GetComponent<RectTransform>().rect.width - 6.5f) / 8.0f;
                // itemWidth = 94.5f;
                __instance.m_Shift = itemWidth;

                var currentViews = __instance.GetComponentsInChildren<PartyCharacterPCView>(true);
                List<GameObject> toTweak = new(currentViews.Select(view => view.gameObject));
                firstX = toTweak[0].transform.localPosition.x - 9.5f;

                UpdateCharacterBindings(__instance);

            } catch (Exception e) {
                Main.log.Log("party view initialize\n" + e.ToString());
            }
        }

        [HarmonyPatch(nameof(PartyPCView.UpdateCharacterBindings)), HarmonyPostfix]
        static void UpdateCharacterBindings(PartyPCView __instance) {
            if (PartyVM_Patches.SupportedSlots == 6) return;

            var currentViews = __instance.GetComponentsInChildren<PartyCharacterPCView>(true);
            List<GameObject> toTweak = new(currentViews.Select(view => view.gameObject));
            firstX = toTweak[0].transform.localPosition.x - 9.5f;

            for (int i = 0; i < toTweak.Count; i++) {
                GameObject view = toTweak[i];

                TweakPCView(__instance, i, view);
            }
        }

        private static void TweakPCView(PartyPCView __instance, int i, GameObject view) {
            var viewRect = view.transform as RectTransform;

            if (viewRect.localScale.x <= (scale + 0.01f)) return;

            var pos = viewRect.localPosition;
            pos.x = firstX + (i * itemWidth);
            viewRect.localPosition = pos;
            viewRect.localScale = new Vector3(scale, scale, 1);

            var portraitRect = view.transform.Find("Portrait") as RectTransform;
            const float recaleFactor = 1.25f;
            portraitRect.localScale = new Vector3(recaleFactor, recaleFactor, 1);

            var frameRect = view.transform.Find("Frame") as RectTransform;
            frameRect.pivot = new Vector2(.5f, 1);
            frameRect.anchoredPosition = new Vector2(0, 23);
            frameRect.sizeDelta = new Vector2(0, 47);

            var healthBarRect = view.transform.Find("Health") as RectTransform;
            healthBarRect.pivot = new Vector2(0, 1);
            healthBarRect.anchoredPosition = new Vector2(0, -2);
            healthBarRect.anchorMin = new Vector2(0, 1);
            healthBarRect.anchorMax = new Vector2(0, 1);
            healthBarRect.localScale = new Vector2(recaleFactor, recaleFactor);

            var encumbranceRect = view.transform.Find("EncumbranceIndicator") as RectTransform;
            encumbranceRect.anchoredPosition = new Vector2(0, -1);
            encumbranceRect.anchorMin = new Vector2(1, 1);
            encumbranceRect.anchorMax = new Vector2(1, 1);
            encumbranceRect.localScale = new Vector2(recaleFactor, recaleFactor);

            var coordDummy = healthBarRect.Find("Dummy") as RectTransform;
            if (coordDummy == null) {
                coordDummy = new GameObject("CoordDummy", typeof(RectTransform)).transform as RectTransform;
                coordDummy.parent = healthBarRect;
            }
            coordDummy.anchorMin = new Vector2(1, 0);
            coordDummy.anchorMax = new Vector2(1, 0);
            coordDummy.anchoredPosition = new Vector2(0, 0);

            var hitpointRect = view.transform.Find("BottomBlock") as RectTransform;
            hitpointRect.localScale = new Vector3(recaleFactor, recaleFactor, 1);
            Vector3[] corners = new Vector3[4];
            hitpointRect.GetWorldCorners(corners);
            Vector3 newPos = coordDummy.position + (hitpointRect.position - corners[0]);
            hitpointRect.position = newPos;

            view.transform.Find("PartBuffView").gameObject.SetActive(false);

            (view.transform.Find("Frame/Selected/Mark") as RectTransform).anchoredPosition = new Vector2(0, 94);

            var buffRect = view.transform.Find("BuffMain") as RectTransform;

            buffRect.sizeDelta = new Vector2(-8, 24);
            buffRect.pivot = Vector2.zero;
            buffRect.anchorMin = new Vector2(0, 1);
            buffRect.anchorMax = new Vector2(1, 1);
            buffRect.anchoredPosition = new Vector2(4, -4);
            buffRect.Edit<GridLayoutGroupWorkaround>(g => {
                g.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                g.padding.top = 2;
            });
            buffRect.gameObject.AddComponent<Image>().color = new Color(.05f, .05f, .05f);

            var buffHover = buffRect.Find("BuffTriggerNotification").GetComponent<OwlcatSelectable>();

            __instance.AddDisposable(buffHover.OnHoverAsObservable().Subscribe<bool>(selected => {
                viewRect.SetAsLastSibling();
            }));

            buffRect.Find("BuffTriggerNotification/BuffAdditional/").localScale = new Vector2(7.0f / 8, 7.0f / 8);
        }
        public static T Edit<T>(this Transform obj, Action<T> build) where T : Component {
            var component = obj.GetComponent<T>();
            build(component);
            return component;
        }
    }
}