﻿using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof (ES_SelfHurtCrit))]
    [FriendOf(typeof (ES_SelfHurtCrit))]
    public static partial class ES_SelfHurtCritSystem
    {
        [EntitySystem]
        private static void Awake(this ES_SelfHurtCrit self, Transform transform)
        {
            self.uiTransform = transform;
            self.collector = transform.GetComponent<ReferenceCollector>();
        }

        [EntitySystem]
        private static void Destroy(this ES_SelfHurtCrit self)
        {
            self.hurtInfo = default;
            self.sequenceLeft?.Kill();
            self.sequenceRight?.Kill();
            GameObjectPoolHelper.ReturnTransformToPool(self.uiTransform);
            self.DestroyWidget();
        }

        public static void Initliaze(this ES_SelfHurtCrit self, long caster, HurtProto proto)
        {
            var view = self.Root().GetComponent<UIComponent>().GetDlgLogic<UIHud>().View;
            if (UnitHelper.IsMainUnit(self, caster))
            {
                self.uiTransform.SetParent(view.EG_SelfRectTransform, false);
            }
            else
            {
                self.uiTransform.SetParent(view.EG_OtherRectTransform, false);
            }

            self.caster = caster;
            self.hurtInfo = proto;
            self.sequenceLeft = TweenManager.Instance.CreateTweener<Sequence>();
            self.sequenceRight = TweenManager.Instance.CreateTweener<Sequence>();

            var curve = self.collector.Get<AnimCurveContainer>("Curve").Curves[0];
            Tweener scaleL = self.E_TextText.rectTransform.DOScale(1, 0.24f, 0.4f).SetCurve(curve);
            Tweener moveXL2 = self.E_TextText.rectTransform.DOAnchoredPositionX(-150, 0.6f, -100).SetEase(Ease.InCubic);
            Tweener moveYL2 = self.E_TextText.rectTransform.DOAnchoredPositionY(50, 0.6f, 80).SetEase(Ease.InCubic);
            Tweener alphaL = self.collector.Get<CanvasGroup>("CanvasGroup").DOFade(0, 0.6f).SetEase(Ease.InQuad);
            self.sequenceLeft.Append(scaleL);
            self.sequenceLeft.Append(moveXL2);
            self.sequenceLeft.Join(moveYL2);
            self.sequenceLeft.Join(alphaL);
            self.sequenceLeft.SetAutoSkill(false);
            self.sequenceLeft.OnStart += self.OnPlay;
            self.sequenceLeft.OnUpdated += self.OnUpdate;
            self.sequenceLeft.OnComplete += self.OnComplete;

            Tweener scaleR = self.E_TextText.rectTransform.DOScale(1, 0.24f, 0.4f).SetCurve(curve);
            Tweener moveXR2 = self.E_TextText.rectTransform.DOAnchoredPositionX(150, 0.6f, 100).SetEase(Ease.InCubic);
            Tweener moveYR2 = self.E_TextText.rectTransform.DOAnchoredPositionY(50, 0.6f, 80).SetEase(Ease.InCubic);
            Tweener alphaR = self.collector.Get<CanvasGroup>("CanvasGroup").DOFade(0, 0.6f).SetEase(Ease.InQuad);
            self.sequenceRight.Append(scaleR);
            self.sequenceRight.Append(moveXR2);
            self.sequenceRight.Join(moveYR2);
            self.sequenceRight.Join(alphaR);
            self.sequenceRight.SetAutoSkill(false);
            self.sequenceRight.OnStart += self.OnPlay;
            self.sequenceRight.OnUpdated += self.OnUpdate;
            self.sequenceRight.OnComplete += self.OnComplete;
        }

        private static void OnPlay(this ES_SelfHurtCrit self, Tweener tweener)
        {
            Vector2 pos = Vector2.one;
            UIHelper.WorldToUI(self.startWorldPos, ref pos);
            pos = Vector3.up * Random.Range(-1f, 1f);
            (self.uiTransform as RectTransform).anchoredPosition = pos;
            self.E_ParryExtendImage.SetActive(self.hurtInfo.IsDirect);
            self.E_TextText.text = self.hurtInfo.Hurt.ToString();
        }

        private static void OnUpdate(this ES_SelfHurtCrit self, Tweener tweener)
        {
            Vector2 pos = Vector2.one;
            UIHelper.WorldToUI(self.startWorldPos, ref pos);
            (self.uiTransform as RectTransform).anchoredPosition = pos;
        }

        private static void OnComplete(this ES_SelfHurtCrit self, Tweener tweener)
        {
            tweener?.Reset();
            self.Dispose();
        }

        public static void Play(this ES_SelfHurtCrit self)
        {
            Unit unit = UnitHelper.GetUnitFromCurrentScene(self.Scene(), self.hurtInfo.Id);
            if (!unit)
            {
                self.OnComplete(default);
                return;
            }

            self.startWorldPos = unit.GetComponent<UnitGoComponent>().GetBone(UnitBone.Body).position;
            if (UIHelper.IsCastRight(self.Scene(), self.hurtInfo.Id, self.caster))
            {
                self.sequenceLeft.PlayForward();
            }
            else
            {
                self.sequenceRight.PlayForward();
            }
        }
    }
}