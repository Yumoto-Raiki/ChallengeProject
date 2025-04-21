using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// �I�u�W�F�N�g��z�u���邽�߂̃c�[��
/// ����
/// 2025/4/20
/// </summary>
public class SetObject : EditorWindow
{

    [SerializeField] private VisualTreeAsset _rootVisualTreeAsset;
    [SerializeField] private StyleSheet _rootStyleSheet;

    /// <summary>
    /// �R�s�[����I�u�W�F�N�g
    /// </summary>
    private GameObject _copyObj = default;

    /// <summary>
    /// �C�x���g
    /// </summary>
    private static Event _e = default;


    /// <summary>
    /// �N���b�N���[�h�̃t���O
    /// </summary>
    private bool _isClickMode = false;

    [MenuItem("SetObject/�I�u�W�F�N�g�ݒu")] // �w�b�_���j���[��/�w�b�_�ȉ��̃��j���[��
    private static void ShowWindow()
    {
        var window = GetWindow<SetObject>("UIElements");
        window.titleContent = new GUIContent("SetObject"); // �G�f�B�^�g���E�B���h�E�̃^�C�g��
        window.Show();

    }

    private void CreateGUI()
    {

        _rootVisualTreeAsset.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(_rootStyleSheet);
        var objectField = new ObjectField("Select a GameObject")
        {
            objectType = typeof(GameObject),
            allowSceneObjects = false // �V�[�����̃I�u�W�F�N�g��s����
        };
        InitialClickMode();
        SceneView.duringSceneGui += OnSceneGUI;
        InitialWallFix();

    }

    private void OnSceneGUI(SceneView sceneView)
    {

        OnSetObject();

    }

    /// <summary>
    /// �N���b�N���[�h�N���E��~����
    /// </summary>
    private void InitialClickMode()
    {

        // �{�^�����擾�������ꂽ�Ƃ��p�Ɋ֐���o�^
        Button clickButton = (Button)rootVisualElement.Q<Button>("ClickModeButton");
        clickButton.clicked += () =>
        {

            GameObject oldCopyObj = _copyObj;
            _copyObj = (GameObject)rootVisualElement.Q<ObjectField>("CopyObject").value;
            if ((_copyObj != oldCopyObj || !_isClickMode) && _copyObj != null)
            {

                Debug.Log("<color='red'>�N���b�N���[�h�J�n</color>");
                rootVisualElement.Q<Label>("ClickModeRunning").text = "�ғ���";
                
                _isClickMode = true;
                return;
            }
            Debug.Log("<color='red'>�N���b�N���[�h�I��</color>");
            rootVisualElement.Q<Label>("ClickModeRunning").text = "��~��";
            _isClickMode = false;

        };

    }

    /// <summary>
    ///�ǂ��ς��Č��Ԃ𖄂߂鏈��
    /// </summary>
    private void InitialWallFix()
    {

        // �{�^�����擾�������ꂽ�Ƃ��p�Ɋ֐���o�^
        Button clickButton = (Button)rootVisualElement.Q<Button>("FixButton");
        clickButton.clicked += () =>
        {

            List<GameObject> objs = new List<GameObject>();
            foreach (var objct in Selection.objects)
            {
                if(!(objct is GameObject obj))
                {

                    continue;

                }
                objs.Add(obj);
            }
            for (int i = 0; i < objs.Count; i++)
            {

                Vector3 pos = objs[i].transform.position;
                // ����
                float yPos = pos.y;
                // ������0�ɂ��đ��
                pos.y = 0;
                objs[i].transform.position = pos;

                Vector3 scale = objs[i].transform.localScale;
                // �����𕝂ɕϊ�
                scale.y = yPos / 2 + 1;
                objs[i].transform.localScale = scale;

            }

        };

    }

    /// <summary>
    /// �N���b�N���[�h
    /// </summary>
    /// <param name="isRunning"></param>
    private void OnSetObject()
    {

        if (!_isClickMode)
        {

            return;

        }
        // �C�x���g�擾�������������Ɍ���
        _e = Event.current;
        if (_e.type != EventType.MouseDown || _e.button != 0 || _e == null || _e.alt)
        {

            return;

        }
        // �ݒu�����擾
        int setCount = rootVisualElement.Q<IntegerField>("SetCount").value;
        // �}�E�X�J�[�\���̈ʒu����Ray��ł�
        Ray ray = HandleUtility.GUIPointToWorldRay(_e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // �q�b�g�����ʒu�Ɩ@�����擾
            Vector3 hitPoint = hit.point;
            Vector3 normal = hit.normal;
            // �������ď��������I������I�u�W�F�N�g������
            Transform oldInstanTrans = default;
            if (_copyObj != null)
            {

                for (int i = 1; i <= setCount; i++)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(_copyObj) as GameObject;
                    if (instance != null)
                    {

                        Vector3 pos = hit.collider.transform.position;
                        Vector3 scale = hit.collider.transform.localScale;
                        //if (oldInstanTrans != null)
                        //{

                        //    pos = oldInstanTrans.position;
                        //    scale = oldInstanTrans.localScale;

                        //}
                        // �m�[�}���C�Y�����̃T�C�Y�̂ݎc��
                        scale.x *= normal.x;
                        scale.y *= normal.y;
                        scale.z *= normal.z;
                        Debug.Log(scale+"�T�C�Y");
                        Debug.Log(normal+"�@��");
                        // �@�����Ƀ��[�v���ɂP�u���b�N���炷.
                        // �X�P�[�������a�̂��߂Q�{�ɂ���
                        pos += (scale * 2) * i;
                        // �q�b�g�����I�u�W�F�N�g�̈ʒu����A
                        instance.transform.position = pos;
                        Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");
                        oldInstanTrans = instance.transform;

                    }

                }
            }
        }

    }

}
