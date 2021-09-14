using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using System;

public class NN_Models : MonoBehaviour
{
    // Models
    public NNModel Decoder_onnx;
    public NNModel HF_onnx;
    public NNModel LF_onnx;
    public WorkerFactory.Type runType = WorkerFactory.Type.ComputePrecompiled;
    public float LF_resampling_std = 0f;

    // DataDims
    private int poseDim = 134;
    private int featureDim = 48; //66;
    private int latentDim = 25;
    private int hiddenDim = 100;


    // Workers
    private IWorker Decoder;
    private IWorker HF;
    private IWorker LF;

    // Inputs to LF
    private Tensor LastKeyframeFeature;
    private Tensor CurrentKeyframeFeature;
    private Tensor CurrentFeature;
    private Tensor LastLatent;
    private Tensor NextLatent;

    // Outputs of LF and inputs to HF
    private float Time;
    private float deltaT = SkeletonController.deltaT;
    private Tensor hidden1;
    private Tensor hidden2;
    private Tensor hidden3;

    // Input to Decoder
    private Tensor CurrentLatent;

    private float[] LF_noise;
    
    void Start()
    {

        Model model1 = ModelLoader.Load(Decoder_onnx);
        Decoder = WorkerFactory.CreateWorker(runType, model1);

        Model model2 = ModelLoader.Load(LF_onnx);
        LF = WorkerFactory.CreateWorker(runType, model2);

        Model model3 = ModelLoader.Load(HF_onnx);
        HF = WorkerFactory.CreateWorker(runType, model3);

        // Stored Tensors

        LastKeyframeFeature = new Tensor(1, featureDim, new float[featureDim]);
        CurrentKeyframeFeature = new Tensor(1, featureDim, new float[featureDim]);
        CurrentFeature = new Tensor(1, featureDim, new float[featureDim]);
        LastLatent = new Tensor(1, latentDim, new float[latentDim]);
        NextLatent = new Tensor(1, latentDim, new float[latentDim]);

        Time = 0;
        
        hidden1 = new Tensor(1, hiddenDim, new float[hiddenDim]);
        hidden2 = new Tensor(1, hiddenDim, new float[hiddenDim]);
        hidden3 = new Tensor(1, hiddenDim, new float[hiddenDim]);

        CurrentLatent = new Tensor(1, featureDim, new float[featureDim]);

        LF_noise = new float[latentDim];
        for (int i = 0; i < LF_noise.Length; ++i)
        {
            LF_noise[i] = Util.RandomGaussian(0f, 1f) * LF_resampling_std;
        }

    }

    public float[] getNextLatent()
    {
        return TensorExtensions.AsFloats(NextLatent);
    }

    //public float[] getTrajectoryFromInput(float[] input)
    //{
    //    var inputs = new Dictionary<string, Tensor>();
    //    inputs["Input"] = new Tensor(1, inputDim, input);
    //    inputs["hidden1_in"] = hidden1_trajectory;
    //    inputs["hidden2_in"] = hidden2_trajectory;

    //    Trajectory.Execute(inputs);
    //    hidden1_trajectory.Dispose();
    //    hidden2_trajectory.Dispose();
    //    inputs["Input"].Dispose();

    //    Tensor T_output = Trajectory.CopyOutput("Output");
    //    hidden1_trajectory = Trajectory.CopyOutput("hidden1_out");
    //    hidden2_trajectory = Trajectory.CopyOutput("hidden2_out");

    //    float[] output = TensorExtensions.AsFloats(T_output);
    //    T_output.Dispose();

    //    return output;
    //}

    public float[] Decode(float[]? lat = null){

        if (lat != null)
        {
            CurrentLatent.Dispose();
            CurrentLatent = new Tensor(1, latentDim, lat);
        }

        var inputs = new Dictionary<string, Tensor>();
        inputs["Latent"] = CurrentLatent;
        Decoder.Execute(inputs);
        
        Tensor T_output = Decoder.PeekOutput("Pose"); 
        float[] ret = TensorExtensions.AsFloats(T_output);

        T_output.Dispose();   

        return ret;  
    }

    public void LowFrequency(float[] newFeature){

        var inputs = new Dictionary<string, Tensor>();

        // Shuffle
        LastLatent.Dispose();
        LastLatent = NextLatent;
        LastKeyframeFeature.Dispose();
        LastKeyframeFeature = CurrentKeyframeFeature;
        CurrentKeyframeFeature = new Tensor(1, newFeature.Length, newFeature);

        inputs["LastFeature"] = LastKeyframeFeature;
        inputs["CurrentFeature"] = CurrentKeyframeFeature;
        inputs["CurrentLatent"] = LastLatent;

        //Debug.Log(LastKeyframeFeature.length);
        //Debug.Log(CurrentKeyframeFeature.length);
        LF.Execute(inputs);

        NextLatent = LF.CopyOutput("NextLatent");

        for (int i = 0; i < LF_noise.Length; ++i)
        {
            LF_noise[i] = Util.RandomGaussian(0f, 1f) * LF_resampling_std;
        }
        
    }

    public void HighFrequency(float[] newFeature)
    {
        CurrentFeature.Dispose();
        CurrentFeature = new Tensor(1, newFeature.Length, newFeature);

        float[] nxt = TensorExtensions.AsFloats(NextLatent);
        for (int i = 0; i < LF_noise.Length; ++i)
        {
            nxt[i] = nxt[i] + LF_noise[i] * (1 - Time);
        }

        NextLatent.Dispose();
        NextLatent = new Tensor(1, nxt.Length, nxt);

        var inputs = new Dictionary<string, Tensor>();
        inputs["Feature"] = CurrentFeature;
        inputs["LatentLast"] = LastLatent;
        inputs["LatentNext"] = NextLatent;
        inputs["Time"] = new Tensor(1, 1, new float[] { Time });
        inputs["hidden1_in"] = hidden1;
        inputs["hidden2_in"] = hidden2;
        inputs["hidden3_in"] = hidden3;

        HF.Execute(inputs);

        hidden1.Dispose();
        hidden2.Dispose();
        hidden3.Dispose();
        inputs["Time"].Dispose();

        CurrentLatent.Dispose();
        CurrentLatent = HF.CopyOutput("Output");

        hidden1 = HF.CopyOutput("hidden1_out");
        hidden2 = HF.CopyOutput("hidden2_out");
        hidden3 = HF.CopyOutput("hidden3_out");

        Time = (Time + 1.0f / deltaT) % 1;
    }

    public float[] getLatent()
    {
        return TensorExtensions.AsFloats(CurrentLatent);
    }

    public float[] getLFMode()
    {
        return TensorExtensions.AsFloats(LF.PeekOutput("Mode"));
    }

    //~NN_Models()
    //{
    //    Decoder.Dispose();
    //    HF.Dispose();
    //    LF.Dispose();
    //}

}