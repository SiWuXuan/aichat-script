using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebGLSupport;
using System.IO;
using static LLM;
using UnityEditor;
using static ChatSample;
using System;
using TMPro;
using UnityEngine.TextCore.Text;

public class ChatSample : MonoBehaviour
{
    /// <summary>
    /// ��������
    /// </summary>
    [SerializeField] private ChatSetting m_ChatSettings;
    #region ui
    /// <summary>
    /// ����UI��
    /// </summary>
    [SerializeField] private GameObject m_ChatPanel;
    /// <summary>
    /// �������Ϣ
    /// </summary>
    [SerializeField] public InputField m_InputWord;
    /// <summary>
    /// ���ص���Ϣ
    /// </summary>
    [SerializeField] private Text m_TextBack;
    /// <summary>
    /// ��������
    /// </summary>
    [SerializeField] private AudioSource m_AudioSource;
    


    #endregion

    #region ʾ�����ﶯ��
    /// <summary>
    /// ����������
    /// </summary>
    [SerializeField] private Animator m_Animator;

    #endregion
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            SendData();
        }
    }
    private void Awake()
    {
        //m_CommitMsgBtn.onClick.AddListener(delegate { SendData(); });
        RegistButtonEvent();
        InputSettingWhenWebgl();
        SettingInitialize();
    }

    #region ��Ϣ����

    /// <summary>
    /// webglʱ����֧����������
    /// </summary>
    private void InputSettingWhenWebgl()
    {
#if UNITY_WEBGL
        m_InputWord.gameObject.AddComponent<WebGLSupport.WebGLInput>();
#endif
    }


    /// <summary>
    /// ������Ϣ
    /// </summary>
    public void SendData()
    {
        if (m_InputWord.text.Equals(""))
            return;

        //��Ӽ�¼����
        m_ChatHistory.Add(m_InputWord.text);
        //��ʾ��
        string _msg = m_InputWord.text;

        //��������
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "����˼����...";

        //�л�˼������
        SetAnimator("state", 1);
    }
    /// <summary>
    /// �����ַ���
    /// </summary>
    /// <param name="_postWord"></param>
    public void SendData(string _postWord)
    {
        if (_postWord.Equals(""))
            return;

        //��Ӽ�¼����
        m_ChatHistory.Add(_postWord);
        //��ʾ��
        string _msg = _postWord;

        //��������
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "����˼����...";

        //�л�˼������
        SetAnimator("state", 1);
    }

    /// <summary>
    /// AI�ظ�����Ϣ�Ļص�
    /// </summary>
    /// <param name="_response"></param>
    private void CallBack(string _response)
    {
        _response = _response.Trim();
        m_TextBack.text = "";


        //��¼����
        m_ChatHistory.Add(_response);

        if (m_ChatSettings.m_TextToSpeech == null)
            return;

        m_ChatSettings.m_TextToSpeech.Speak(_response, PlayVoice);
    }

#endregion

#region ��������
    /// <summary>
    /// ����ʶ�𷵻ص��ı��Ƿ�ֱ�ӷ�����LLM
    /// </summary>
    [SerializeField] private bool m_AutoSend = true;
    /// <summary>
    /// ��������İ�ť
    /// </summary>
    [SerializeField] private Button m_VoiceInputBotton;
    /// <summary>
    /// ¼����ť���ı�
    /// </summary>
    [SerializeField]private Text m_VoiceBottonText;
    /// <summary>
    /// ¼������ʾ��Ϣ
    /// </summary>
    [SerializeField] private Text m_RecordTips;
    /// <summary>
    /// �������봦����
    /// </summary>
    [SerializeField] private VoiceInputs m_VoiceInputs;
    /// <summary>
    /// ע�ᰴť�¼�
    /// </summary>
    private void RegistButtonEvent()
    {
        if (m_VoiceInputBotton == null || m_VoiceInputBotton.GetComponent<EventTrigger>())
            return;

        EventTrigger _trigger = m_VoiceInputBotton.gameObject.AddComponent<EventTrigger>();

        //��Ӱ�ť���µ��¼�
        EventTrigger.Entry _pointDown_entry = new EventTrigger.Entry();
        _pointDown_entry.eventID = EventTriggerType.PointerDown;
        _pointDown_entry.callback = new EventTrigger.TriggerEvent();

        //��Ӱ�ť�ɿ��¼�
        EventTrigger.Entry _pointUp_entry = new EventTrigger.Entry();
        _pointUp_entry.eventID = EventTriggerType.PointerUp;
        _pointUp_entry.callback = new EventTrigger.TriggerEvent();

        //���ί���¼�
        _pointDown_entry.callback.AddListener(delegate { StartRecord(); });
        _pointUp_entry.callback.AddListener(delegate { StopRecord(); });

        _trigger.triggers.Add(_pointDown_entry);
        _trigger.triggers.Add(_pointUp_entry);
    }

    /// <summary>
    /// ��ʼ¼��
    /// </summary>
    public void StartRecord()
    {
        m_VoiceBottonText.text = "����¼����..."; 
        m_VoiceInputs.StartRecordAudio();
    }
    /// <summary>
    /// ����¼��
    /// </summary>
    public void StopRecord()
    {
        m_VoiceBottonText.text = "��ס��ť����ʼ¼��"; 
        m_RecordTips.text = "¼������������ʶ��...";
        m_VoiceInputs.StopRecordAudio(AcceptClip);
    }

    /// <summary>
    /// ����¼�Ƶ���Ƶ����
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptData(byte[] _data)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_data, DealingTextCallback);
    }

    /// <summary>
    /// ����¼�Ƶ���Ƶ����
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptClip(AudioClip _audioClip)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_audioClip, DealingTextCallback);
    }
    /// <summary>
    /// ����ʶ�𵽵��ı�
    /// </summary>
    /// <param name="_msg"></param>
    private void DealingTextCallback(string _msg)
    {
        m_RecordTips.text = _msg;
        StartCoroutine(SetTextVisible(m_RecordTips));
        //�Զ�����
        if (m_AutoSend)
        {
            SendData(_msg);
            return;
        }

        m_InputWord.text = _msg;
    }

    private IEnumerator SetTextVisible(Text _textbox)
    {
        yield return new WaitForSeconds(3f);
        _textbox.text = "";
    }

#endregion

#region �����ϳ�

    private void PlayVoice(AudioClip _clip, string _response)
    {
        if(_response == "��Ȩ���������Ƿ���ȷ��дtoken��token�ĸ�ʽӦΪsk- xxxxxxxxxxxxxx")
        {
            //Debug.Log(_response);
            m_WriteState = true;
            StartCoroutine(SetTextPerWord(_response));
        }
        else
        {
            m_AudioSource.clip = _clip;
            m_AudioSource.Play();
            Debug.Log("��Ƶʱ����" + _clip.length);
            //��ʼ�����ʾ���ص��ı�
            m_WriteState = true;
            StartCoroutine(SetTextPerWord(_response));

           
        }
        //�л���˵������
        SetAnimator("state", 2);
    }

#endregion

#region ���������ʾ
    //������ʾ��ʱ����
    [SerializeField] private float m_WordWaitTime = 0.2f;
    //�Ƿ���ʾ���
    [SerializeField] private bool m_WriteState = false;
    private IEnumerator SetTextPerWord(string _msg)
    {
        Debug.Log(_msg);
        int currentPos = 0;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //������ʾ������
            m_TextBack.text = _msg.Substring(0, currentPos);

            m_WriteState = currentPos < _msg.Length;

        }

        //�л����ȴ�����
        SetAnimator("state",0);
    }

    #endregion
    private void SetAnimator(string _para, int _value)
    {
        if (m_Animator == null)
            return;

        m_Animator.SetInteger(_para, _value);
    }

    #region �����¼
    //���������¼
    [SerializeField] private List<string> m_ChatHistory;
    //�����Ѵ�������������
    [SerializeField] private List<GameObject> m_TempChatBox;
    //�����¼��ʾ��
    [SerializeField] private GameObject m_HistoryPanel;
    //�����ı����õĲ�
    [SerializeField] private RectTransform m_rootTrans;
    //������������
    [SerializeField] private ChatPrefab m_PostChatPrefab;
    //�ظ�����������
    [SerializeField] private ChatPrefab m_RobotChatPrefab;
    //������
    [SerializeField] private ScrollRect m_ScroTectObject;
    //��ȡ�����¼
    public void OpenAndGetHistory()
    {
        m_ChatPanel.SetActive(false);
        m_HistoryPanel.SetActive(true);

        ClearChatBox();
        StartCoroutine(GetHistoryChatInfo());
    }
    //����
    public void BackChatMode()
    {
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
        m_SettingPanel.SetActive(false);
    }

    //����Ѵ����ĶԻ���
    private void ClearChatBox()
    {
        while (m_TempChatBox.Count != 0)
        {
            if (m_TempChatBox[0])
            {
                Destroy(m_TempChatBox[0].gameObject);
                m_TempChatBox.RemoveAt(0);
            }
        }
        m_TempChatBox.Clear();
    }

    //��ȡ�����¼�б�
    private IEnumerator GetHistoryChatInfo()
    {

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < m_ChatHistory.Count; i++)
        {
            if (i % 2 == 0)
            {
                ChatPrefab _sendChat = Instantiate(m_PostChatPrefab, m_rootTrans.transform);
                _sendChat.SetText(m_ChatHistory[i]);
                m_TempChatBox.Add(_sendChat.gameObject);
                continue;
            }

            ChatPrefab _reChat = Instantiate(m_RobotChatPrefab, m_rootTrans.transform);
            _reChat.SetText(m_ChatHistory[i]);
            m_TempChatBox.Add(_reChat.gameObject);
        }

        //���¼��������ߴ�
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootTrans);
        StartCoroutine(TurnToLastLine());
    }

    private IEnumerator TurnToLastLine()
    {
        yield return new WaitForEndOfFrame();
        //�������������Ϣ
        m_ScroTectObject.verticalNormalizedPosition = 0;
    }


    #endregion




    #region//���ý������
    //������ʾ��
    [SerializeField] private GameObject m_SettingPanel;
    [SerializeField] private AudioSource m_BGMSource;
    [SerializeField] private GameObject m_normalModel;
    [SerializeField] private GameObject m_ARModel;
    [SerializeField] private GameObject m_normalGroup;
    [SerializeField] private GameObject m_ARGroup;
    //�����ļ�json�洢��ַ
    private string settingURL;
    //UI���
    public Slider voiceVolume_UI;
    public CustomDropdown character_UI;
    public Slider BGMVolume_UI;
    public SwitchManager AR_UI;
    public CustomInputField token_UI;
    public CustomDropdown model_UI;

    [System.Serializable]
    public class SettingData
    {
        public float voiceVolume;
        public OpenAITextToSpeech.VoiceType character;
        public float BGMVolume;
        public bool isARMode;
        public string token;
        public string LLM_Model;
        public SettingData(float voiceVolume=50, OpenAITextToSpeech.VoiceType character=OpenAITextToSpeech.VoiceType.nova, float bGMVolume=50, bool isARMode=false, string token="", string lLM_Model="gpt-3")
        {
            this.voiceVolume = voiceVolume;
            this.character = character;
            BGMVolume = bGMVolume;
            this.isARMode = isARMode;
            this.token = token;
            LLM_Model = lLM_Model;
        }
    }
    public SettingData m_settingData=new SettingData();
    //���ó�ʼ��
    public void SettingInitialize()
    {
        settingURL = Application.persistentDataPath + "\\SettingData.json";
        Debug.Log(settingURL);
        string js = JsonUtility.ToJson(m_settingData);
        if (System.IO.File.Exists(settingURL))
        {
            //��json�ļ�д��m_settingData
            using (StreamReader sr = File.OpenText(settingURL))
            {
                string text=sr.ReadToEnd();
                m_settingData = JsonUtility.FromJson<SettingData>(text);
                sr.Close();
            }
            //Ӧ�õ��ͻ�������
            ChangeCharacterVolume(m_settingData.voiceVolume);
            ChangeUIPerform_Voice(m_settingData.voiceVolume);

            ChangeBGMVolume(m_settingData.BGMVolume);
            ChangeUIPerform_BGM(m_settingData.BGMVolume);

            SetCharacter(m_settingData.character);

            //if (m_settingData.isARMode)
            //{
            //    TransferToAR();
            //}
            //else
            //{
            //    TransfertToNormal();
            //}


            InputToken(m_settingData.token);
            ChangeUIPerform_Token(m_settingData.token);

            SetModel(m_settingData.LLM_Model);
        }
        else
        {
            //��m_settingDataд��json�ļ�
            using (StreamWriter sw = new StreamWriter(settingURL))
            {
                //��������
                sw.WriteLine(js);
                //�ر��ĵ�
                sw.Close();
                sw.Dispose();
            }
        }


    }
    //���������ļ�
    public void UpdateSettingData()
    {
        string js = JsonUtility.ToJson(m_settingData);
        using (StreamWriter sw = new StreamWriter(settingURL))
        {
            //��������
            sw.WriteLine(js);
            //�ر��ĵ�
            sw.Close();
            sw.Dispose();
        }
    }
    //������ҳ��
    public void OpenSettingPanel()
    {
        m_ChatPanel.SetActive(false);
        m_SettingPanel.SetActive(true);
    }
    //���沢�ر�����ҳ��
    public void FinishSetting()
    {
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
        m_SettingPanel.SetActive(false);
        UpdateSettingData();
    }


    //����˵������
    public void ChangeCharacterVolume(float value)
    {
        m_AudioSource.volume = value/100;
        m_settingData.voiceVolume=value;
    }
    public void ChangeUIPerform_Voice(float value)
    {
        voiceVolume_UI.value = value;
    }
    //����BGM����
    public void ChangeBGMVolume(float value)
    {
        m_BGMSource.volume = value/100;
        m_settingData.BGMVolume = value;
    }
    public void ChangeUIPerform_BGM(float value)
    {
        BGMVolume_UI.value = value;
    }
    //ѡ��˵����ɫ
    public void SetCharacter(int value)//�����ͷ���������UI��
    {
        OpenAITextToSpeech.VoiceType characterName= OpenAITextToSpeech.VoiceType.nova;
        switch (value)
        {
            case 0:
                characterName = OpenAITextToSpeech.VoiceType.alloy;
                break;
            case 1:
                characterName = OpenAITextToSpeech.VoiceType.echo;
                break;
            case 2:
                characterName = OpenAITextToSpeech.VoiceType.fable;
                break;
            case 3:
                characterName = OpenAITextToSpeech.VoiceType.onyx;
                break;
            case 4:
                characterName = OpenAITextToSpeech.VoiceType.nova;
                break;
            case 5:
                characterName = OpenAITextToSpeech.VoiceType.shimmer;
                break;
        }
        TTS tts = m_ChatSettings.m_TextToSpeech;
        if(tts is OpenAITextToSpeech)
        {
            (tts as OpenAITextToSpeech).m_Voice = characterName;
            m_settingData.character = characterName;
        }
    }
    public void SetCharacter(OpenAITextToSpeech.VoiceType character)//��ö���ͷ��������ڳ�ʼ����
    {
        TTS tts = m_ChatSettings.m_TextToSpeech;
        if (tts is OpenAITextToSpeech)
        {
            (tts as OpenAITextToSpeech).m_Voice = character;
            m_settingData.character = character;
        }
        character_UI.SetDropdownIndex((int)character);
    }
    //�л���ARģʽ
    public void TransferToAR()
    {
        m_normalGroup.SetActive(false);
        m_ARGroup.SetActive(true);      
        m_Animator= m_ARModel.GetComponent<Animator>();
        m_settingData.isARMode = true;
    }
    //�л�����ͨģʽ
    public void TransfertToNormal()
    {
        m_ARGroup.SetActive(false);
        m_normalGroup.SetActive(true);
        m_Animator = m_normalModel.GetComponent<Animator>();
        m_settingData.isARMode = false;
    }
    //����token
    public void InputToken(string token)
    {
        m_settingData.token = token;    
        if (m_ChatSettings.m_ChatModel is chatgptTurbo)
        {
            (m_ChatSettings.m_ChatModel as chatgptTurbo).api_key = token;
        }
        else
        {
            Debug.LogWarning("������LLMΪchatgptTurbo��");
        }
        if(m_ChatSettings.m_TextToSpeech is OpenAITextToSpeech)
        {
            (m_ChatSettings.m_TextToSpeech as OpenAITextToSpeech).api_key=token;
        }
    }
    public void ChangeUIPerform_Token(string token)
    {
        token_UI.inputText.text = token;
        StartCoroutine(setUIAnime_token());
    }
    //ѡ�������ģ��
    public void SetModel(int index)
    {
        if(m_ChatSettings.m_ChatModel is chatgptTurbo)
        {
            chatgptTurbo gpt = m_ChatSettings.m_ChatModel as chatgptTurbo;
            string modelSelector="";
            switch (index)
            {
                case 0:
                    modelSelector = "gpt-3";
                    break;
                case 1:
                    modelSelector = "spark";
                    break;
                case 2:
                    modelSelector = "baidu";
                    break;
                case 3:
                    modelSelector = "glm";
                    break;
                case 4:
                    modelSelector = "gpt-4";
                    break;
                case 5:
                    modelSelector = "ali";
                    break;
            }
            gpt.m_gptModel=modelSelector;
            m_settingData.LLM_Model = modelSelector;
        }
        else
        {
            Debug.LogWarning("������LLMΪchatgptTurbo��");
        }
    }
    public void SetModel(string modelSelector)
    {
        if (m_ChatSettings.m_ChatModel is chatgptTurbo)
        {
            chatgptTurbo gpt = m_ChatSettings.m_ChatModel as chatgptTurbo;
            gpt.m_gptModel = modelSelector;
            m_settingData.LLM_Model = modelSelector;
        }
        else
        {
            Debug.LogWarning("������LLMΪchatgptTurbo��");
        }
        Dictionary<string, int> map_character = new Dictionary<string, int>()
        {
            {"gpt-3",0 },
            {"spark",1 },
            {"baidu",2 },
            {"glm",3 },
            {"gpt-4",4 },
            {"ali",5 },
        };
        model_UI.SetDropdownIndex(map_character[modelSelector]);
    }



    //��ʱ��������ֹ��UI��ʼ�����ǣ�
    private IEnumerator setUIAnime_token()
    {
        yield return new WaitForSeconds(0.2f);
        token_UI.UpdateStateInstant();
    }
    #endregion


}
