using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;

namespace Presenters.NodePresenters
{

    public class DescriptionActionNodePresenter : ActionNodePresenter
    {

        [SerializeField] private TMP_InputField narrativeText;

        [SerializeField] private FlexibleColorPicker fcp_text;
        [SerializeField] private FlexibleColorPicker fcp_bg;

        [SerializeField] private Button btn_txt;
        [SerializeField] private Button btn_bg;




        public NotifierCanvas nc;
        private Camera cam;
        private GameObject centerEyeAnchor;

        IEnumerator Start()
        {
            // Keep looking for the camera until it's found
            while (cam == null)
            {
                cam = Camera.main;
                if (cam == null)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    centerEyeAnchor = cam.gameObject;
                    nc = centerEyeAnchor.GetComponentInChildren<NotifierCanvas>();

                    break;
                }
            }
        }


        protected override void Awake()
        {
            base.Awake();

            SetActionType(ActionNode.ActionType.DescriptionAction);

            if (btn_txt != null)
            {
                btn_txt.onClick.AddListener(OnButtonClick_Text);
            }

            if (btn_bg != null)
            {
                btn_bg.onClick.AddListener(OnButtonClick_BackGround);
            }

        }

        

        protected override void OnDisable()
        {
            base.OnDisable();

            if (btn_txt != null)
            {
                btn_txt.onClick.RemoveAllListeners();
            }

            if (btn_bg != null)
            {
                btn_bg.onClick.RemoveAllListeners();
            }

        }







        protected override void PerformAction()
        {
            base.PerformAction();

            nc.descriptionPanel.GetComponentInChildren<TMP_Text>().text = narrativeText.text;
            nc.descriptionPanel.GetComponentInChildren<TMP_Text>().color = fcp_text.color;
            nc.descriptionPanel.GetComponent<Image>().color = fcp_bg.color;

            nc.ShowDescriptionPanel();



        }


        private void OnButtonClick_Text()
        {
            if (fcp_text.gameObject.activeSelf == true)
            {
                fcp_text.gameObject.SetActive(false);
            }
            else
            {
                fcp_text.gameObject.SetActive(true);
            }


        }

        private void OnButtonClick_BackGround()
        {
            if (fcp_bg.gameObject.activeSelf == true)
            {
                fcp_bg.gameObject.SetActive(false);
            }
            else
            {
                fcp_bg.gameObject.SetActive(true);
            }
        }

        public void PerformRemove()
        {
            nc.descriptionPanel.SetActive(false);
        }


    }
}
