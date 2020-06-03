using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

// Warning: all parameters will be overwritten at runtime
public class VirusModel : MonoBehaviour
{
    [Header("Virus Timer")]
    /* Virus [R]eproduction calculation:
     * R_in_block = InfectedStage.reproduction  (as GLOBAL R FACTOR, will change with time)
     *            * BlockTypeParameter.R_FACTOR (as LOCAL R FACTOR, block type specific)
     *            + Block.VIRUS_R_FACTOR        (as VIRUS R FACTOR, runtime specific, 
     *                                           change with development of pandemic)
     *                                           
     * At the end of a round, virus will be reproduced in block
     * (that is to say, reproduction in one block is independent 
     *  of other blocks) according to R_in_block. To be specific,
     * infected_population += Random(0, infected_population * R_in_block)
     * Ideally Random() should obey a Poisson distribution or something
     * buf for now it's just uniform. May be modified in the future.
     */
    // InfectedStage(GLOBAL_R_FACTOR, GLOBAL_DR_FACTOR)
    public InfectedStage[] stages = {
         new InfectedStage(0.0f, 0.0f),
         new InfectedStage(1.0f, 0.0f),
         new InfectedStage(1.0f, 0.1f),
         new InfectedStage(1.0f, 0.5f)
     };
    // number of days a stage will last
    public int timerStagePeriod = 3;

    [Header("Virus Effect")]
    // Virus_{k+1} = ( Virus_{k} + VIRUS_SCALING * (NIP + CIP) ) * VIRUS_DECAY
    // VIRUS_R_FACTOR = AMPLITUDE * F( Virus_{k+1} * GRADIENT )
    // when Virus * GRADIENT >= 0.95, the VIRUS_FACTOR  will be nearly AMPLITUDE
    public float virusScaling = 0.1f;
    public float virusDecay = 0.9f;
    public float virusAmplitude = 1.0f;
    public float virusGradient = 0.01f;

    [Header("Mechanic Options")]
    // set this flag to true if you want to generate random population for all blocks
    // the range of random generation is defined in VirusModel
    public bool randomPopulation = true;
    // random population generation parameter
    public int randomPopulationMin = 400;
    public int randomPopulationMax = 600;

    // set this flag to true if you want to auto-collect and manage materials globally
    public bool autoGlobalTaxing = false;

    // set this flag to true if you want to enable Population Volume mechanism
    public bool enableVolumeConstraint = false;

    // set this flag to true if you want to see all infected
    public bool enableGodView = false;

    [Header("Others")]
    // set this flag to true if you want to use UI version 2
    // UI version 1 for False
    public bool enableUIVer2 = true;

    // set this flag to true if you want to display data as health bar
    public bool enableHealthBar = true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/* BlockTypeParameter
 * important parameters that are strongly (or at least potentially) related to block type
 * though some of them may be temporarily the same for all types of blocks
 *  ============= WARNING =============
 *  All parameters coded here will be overwritten by data set in the inspector.
 *  Value here are just for reference.
 */
[System.Serializable]
public class BlockTypeParameter
{

    [System.NonSerialized]
    private static Dictionary<BlockType, BlockTypeParameter> parameters = new Dictionary<BlockType, BlockTypeParameter>();
    public static Dictionary<BlockType, BlockTypeParameter> Parameters
    {
        get
        {
            return parameters;
        }
    }
    public static BlockTypeParameter GetParameter(BlockType type)
    {
        if (parameters.ContainsKey(type))
        {
            return parameters[type];
        }   
        Debug.Log("WARNING: (BlockTypeParameter:GetParameter) parameter not found. New entry created.");
        parameters[type] = new BlockTypeParameter(type);
        return parameters[type];
    }

    [System.NonSerialized]
    public BlockType type;

    public float RFactor { get { return R_FACTOR; } set { R_FACTOR = value; } }
    public float DFactor { get { return D_FACTOR; } set { D_FACTOR = value; } }
    public float ComsumeFactor { get { return CONSUME_FACTOR; } set { CONSUME_FACTOR = value; } }
    public float ProduceFactor { get { return PRODUCE_FACTOR; } set { PRODUCE_FACTOR = value; } }
    public float TaxRate { get { return TAX_RATE; } set { TAX_RATE = value; } }
    public float HPMoveRatio { get { return HP_MOVE_RATIO; } set { HP_MOVE_RATIO = value; } }
    public float CIPMoveRatio { get { return CIP_MOVE_RATIO; } set { CIP_MOVE_RATIO = value; } }
    public float NIPMoveRatio { get { return NIP_MOVE_RATIO; } set { NIP_MOVE_RATIO = value; } }
    public float MMoveRatio { get { return M_MOVE_RATIO; } set { M_MOVE_RATIO = value; } }
    public float VirusMoveRatio { get { return VIRUS_MOVE_RATIO; } set { VIRUS_MOVE_RATIO = value; } }
    public float CIPPriorityOffset { get { return CIP_PRIORITY_OFFSET; } set { CIP_PRIORITY_OFFSET = value; } }
    public int PopulationVolume { get { return POPULATION_VOLUME; } set { POPULATION_VOLUME = value; } }
    
    // reproduction rate factor
    public float R_FACTOR = 2.0f;
    // death rate factor
    public float D_FACTOR = 1.0f;
    // material consuming rate factor
    public float CONSUME_FACTOR = -0.01f;
    // material producing rate factor (FOR FACTORY)
    public float PRODUCE_FACTOR = +1f;
    // tax rate
    public float TAX_RATE = 0.05f;
    // ratio by which healthy population will move out at most
    public float HP_MOVE_RATIO = 0.05f;
    // ratio by which current generation of infected population will move out at most
    public float CIP_MOVE_RATIO = 0.01f;
    // ratio by which next generation of infected population will move out at most
    public float NIP_MOVE_RATIO = 0.05f;
    // ratio by which material will move out at most
    public float M_MOVE_RATIO = 0.5f;
    // ratio by which virus will move out at most
    public float VIRUS_MOVE_RATIO = 0.5f;

    public float CIP_PRIORITY_OFFSET = 0.0f;
    // population constraint if population volume enabled
    public int POPULATION_VOLUME = 550;

    [System.NonSerialized]
    public int RESOURCE_MIN = 0;

    public object GetVal(string property)
    {
        if (property == "type")
        {
            return Utility.BlockTypeToString(type);
        }
        return this.GetType().GetProperty(property).GetValue(this);
    }
    public void SetVal(string property, string value)
    {
        // todo: parse string to value here
        if (property == "type") return;
        var propertyInfo = GetType().GetProperty(property);
        var type = propertyInfo.PropertyType;
        if (type == typeof(float))
        {
            try
            {
                float res = float.Parse(value);
                propertyInfo.SetValue(this, res);
            }
            catch
            {
                Debug.Log(string.Format("ERROR: (SetVal) invalid csv data {0} for float {1}", value, property));
                return;
            }
        } else if (type == typeof(int))
        {
            try
            {
                int res = int.Parse(value);
                propertyInfo.SetValue(this, res);
            }
            catch
            {
                Debug.Log(string.Format("ERROR: (SetVal) invalid csv data {0} for float {1}", value, property));
                return;
            }
        } else if (type == typeof(string)) 
        {
            propertyInfo.SetValue(this, value);
        }
        else// else what?
        {
            Debug.Log(string.Format("what's this type of data? {0} -> {1}", type.Name, property));
        }
    }
    public static List<string> GetPropertyList()
    {
        List<string> ret = new List<string>();
        foreach (var ele in typeof(BlockTypeParameter).GetProperties())
        {
            if (ele.PropertyType.IsValueType)
            {
                ret.Add(ele.Name);
            }
        }
        return ret;
    }
    public List<string> GetValList(List<string> properties)
    {
        List<string> ret = new List<string>();
        foreach (var ele in properties)
        {
            ret.Add(GetVal(ele).ToString());
        }
        return ret;
    }
    public void SetValList(Dictionary<string, string> values)
    {
        foreach (var pair in values)
        {
            SetVal(pair.Key, pair.Value);
        }
    }

    public BlockTypeParameter(BlockType mType)
    {
        type = mType;
        if (type == BlockType.HOSPITAL)
        {
            R_FACTOR = 0.5f;
            D_FACTOR = 0.0f;
            CONSUME_FACTOR = -0.1f;
            CIP_PRIORITY_OFFSET = 2.0f;
            POPULATION_VOLUME = 900;
        }
        if (type == BlockType.QUARANTINE)
        {
            POPULATION_VOLUME = 1500;
        }
        if (type == BlockType.FACTORY)
        {
            POPULATION_VOLUME = 100;
        }
    }
    
}

public class FileParameterManager
{
    // path to block parameters
    public static string dataPath;
    public static void Init()
    {
        dataPath = Application.streamingAssetsPath + "/model_data.csv";
    }

    public static void DumpData()
    {
        List<List<string>> data = new List<List<string>>();
        List<string> props = BlockTypeParameter.GetPropertyList();
        props.Insert(0, "type");
        data.Add(props);
        foreach (var pair in BlockTypeParameter.Parameters)
        {
            data.Add(pair.Value.GetValList(props));
        }
        CSVTool.Write(dataPath, Encoding.UTF8, data);
        
    }

    public static void LoadData()
    {
        var data = CSVTool.Read(dataPath, Encoding.UTF8);
        var props = data[0];
        if (!props.Contains("type"))
        {
            Debug.Log("ERROR: (LoadData) csv TYPE field missing");
            return;
        }
        for (int j = 1; j < data.Count; ++j)
        {
            if (data[j].Count != props.Count)
            {
                Debug.Log("ERROR: (LoadData) csv data count mismatch");
                continue;
            }
            Dictionary<string, string> values = new Dictionary<string, string>();
            for (int i = 0; i < props.Count; ++i)
            {
                values[props[i]] = data[j][i];
            }
            BlockType type = Utility.StringToBlockType(values["type"]);
            BlockTypeParameter param = BlockTypeParameter.GetParameter(type);
            if (param == null)
            {
                Debug.Log("ERROR: (LoadData) invalid csv TYPE field");
            }
            param.SetValList(values);
            
        }

        var factory = BlockTypeParameter.GetParameter(BlockType.FACTORY);
        Debug.Log(factory.RFactor);
        Debug.Log(factory.DFactor);
        Debug.Log(factory.ComsumeFactor);
        Debug.Log(factory.PopulationVolume);

    }
}

