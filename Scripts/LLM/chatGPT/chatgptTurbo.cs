using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class chatgptTurbo : LLM
{
    public chatgptTurbo()
    {
        //url = "http://api.openai.com/v1/chat/completions";
        url = "https://api.alpacabro.cc/v1/chat/completions";
    }

    /// <summary>
    /// api key
    /// </summary>
    [SerializeField] public string api_key;
    /// <summary>
    /// AI�趨
    /// </summary>
    public string m_SystemSetting = string.Empty;
    /// <summary>
    /// gpt-3.5-turbo
    /// </summary>
    public string m_gptModel = "gpt-3.5-turbo";


    private void Start()
    {
        //����ʱ�����AI�趨
        m_DataList.Add(new SendData("system", m_SystemSetting));
    }
    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <returns></returns>
    public override void PostMsg(string _msg, Action<string> _callback)
    {
        base.PostMsg(_msg, _callback);
    }

    /// <summary>
    /// ���ýӿ�
    /// </summary>
    /// <param name="_postWord"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public override IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        stopwatch.Restart();
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_gptModel,
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));
            
            //test
            //request.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");
            //test/
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                string _msgBack = request.downloadHandler.text;
                Debug.Log(_msgBack);
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                if (_textback != null && _textback.choices.Count > 0)
                {

                    string _backMsg = _textback.choices[0].message.content;
                    Debug.Log(_backMsg);
                    //��Ӽ�¼
                    m_DataList.Add(new SendData("assistant", _backMsg));
                    _callback(_backMsg);
                }

            }else if(request.responseCode == 401)
            {
                m_DataList.Add(new SendData("assistant", "��Ȩ���������Ƿ���ȷ��дtoken��token�ĸ�ʽӦΪsk- xxxxxxxxxxxxxx"));
                _callback("��Ȩ���������Ƿ���ȷ��дtoken��token�ĸ�ʽӦΪsk- xxxxxxxxxxxxxx");
            }

            stopwatch.Stop();
            Debug.Log("chatgpt��ʱ��"+ stopwatch.Elapsed.TotalSeconds);
        }
    }

    #region ���ݰ�

    [Serializable]
    public class PostData
    {
        public string model;
        public List<SendData> messages;
    }

    [Serializable]
    public class MessageBack
    {
        public string id;
        public string created;
        public string model;
        public List<MessageBody> choices;
    }
    [Serializable]
    public class MessageBody
    {
        public Message message;
        public string finish_reason;
        public string index;
    }
    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    #endregion

}
