using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusModel : MonoBehaviour
{

    public InfectedStage[] stages = {
         new InfectedStage(0.0f, 0.0f),
         new InfectedStage(1.0f, 0.0f),
         new InfectedStage(1.0f, 0.1f),
         new InfectedStage(1.0f, 0.5f)
     };
 
    public int timerStagePeriod = 3;

    public BlockTypeParameter factory = new BlockTypeParameter(BlockType.FACTORY);
    public BlockTypeParameter housing = new BlockTypeParameter(BlockType.HOUSING);
    public BlockTypeParameter hospital = new BlockTypeParameter(BlockType.HOSPITAL);
    public BlockTypeParameter quarantine = new BlockTypeParameter(BlockType.QUARANTINE);

    // set this flag to true if you want to generate random population
    // the range of random generation is defined in VirusModel
    public bool randomPopulation = true;
    // random population generation parameter
    public int randomPopulationMin = 400;
    public int randomPopulationMax = 600;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
