using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// オブジェクトを配置するためのツール
/// 湯元
/// 2025/4/20
/// </summary>
public class SetObject : EditorWindow
{

    [SerializeField] private VisualTreeAsset _rootVisualTreeAsset;
    [SerializeField] private StyleSheet _rootStyleSheet;

    /// <summary>
    /// コピーするオブジェクト
    /// </summary>
    private GameObject _copyObj = default;

    /// <summary>
    /// イベント
    /// </summary>
    private static Event _e = default;


    /// <summary>
    /// クリックモードのフラグ
    /// </summary>
    private bool _isClickMode = false;

    [MenuItem("SetObject/オブジェクト設置")] // ヘッダメニュー名/ヘッダ以下のメニュー名
    private static void ShowWindow()
    {
        var window = GetWindow<SetObject>("UIElements");
        window.titleContent = new GUIContent("SetObject"); // エディタ拡張ウィンドウのタイトル
        window.Show();

    }

    private void CreateGUI()
    {

        _rootVisualTreeAsset.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(_rootStyleSheet);
        var objectField = new ObjectField("Select a GameObject")
        {
            objectType = typeof(GameObject),
            allowSceneObjects = false // シーン内のオブジェクトを不許可
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
    /// クリックモード起動・停止処理
    /// </summary>
    private void InitialClickMode()
    {

        // ボタンを取得し押されたとき用に関数を登録
        Button clickButton = (Button)rootVisualElement.Q<Button>("ClickModeButton");
        clickButton.clicked += () =>
        {

            GameObject oldCopyObj = _copyObj;
            _copyObj = (GameObject)rootVisualElement.Q<ObjectField>("CopyObject").value;
            if ((_copyObj != oldCopyObj || !_isClickMode) && _copyObj != null)
            {

                Debug.Log("<color='red'>クリックモード開始</color>");
                rootVisualElement.Q<Label>("ClickModeRunning").text = "稼動中";
                
                _isClickMode = true;
                return;
            }
            Debug.Log("<color='red'>クリックモード終了</color>");
            rootVisualElement.Q<Label>("ClickModeRunning").text = "停止中";
            _isClickMode = false;

        };

    }

    /// <summary>
    ///壁を均して隙間を埋める処理
    /// </summary>
    private void InitialWallFix()
    {

        // ボタンを取得し押されたとき用に関数を登録
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
                // 半分
                float yPos = pos.y;
                // 高さを0にして代入
                pos.y = 0;
                objs[i].transform.position = pos;

                Vector3 scale = objs[i].transform.localScale;
                // 高さを幅に変換
                scale.y = yPos / 2 + 1;
                objs[i].transform.localScale = scale;

            }

        };

    }

    /// <summary>
    /// クリックモード
    /// </summary>
    /// <param name="isRunning"></param>
    private void OnSetObject()
    {

        if (!_isClickMode)
        {

            return;

        }
        // イベント取得しそれらを条件に検査
        _e = Event.current;
        if (_e.type != EventType.MouseDown || _e.button != 0 || _e == null || _e.alt)
        {

            return;

        }
        // 設置個数を取得
        int setCount = rootVisualElement.Q<IntegerField>("SetCount").value;
        // マウスカーソルの位置からRayを打つ
        Ray ray = HandleUtility.GUIPointToWorldRay(_e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // ヒットした位置と法線を取得
            Vector3 hitPoint = hit.point;
            Vector3 normal = hit.normal;
            // 生成して処理をし終わったオブジェクトを入れる
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
                        // ノーマライズ方向のサイズのみ残す
                        scale.x *= normal.x;
                        scale.y *= normal.y;
                        scale.z *= normal.z;
                        Debug.Log(scale+"サイズ");
                        Debug.Log(normal+"法線");
                        // 法線側にループ毎に１ブロックずらす.
                        // スケールが半径のため２倍にする
                        pos += (scale * 2) * i;
                        // ヒットしたオブジェクトの位置から、
                        instance.transform.position = pos;
                        Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");
                        oldInstanTrans = instance.transform;

                    }

                }
            }
        }

    }

}
