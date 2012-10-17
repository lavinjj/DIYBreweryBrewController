using System;
using System.Collections;
using CodingSmackdown.Services;
using FastloadMedia.NETMF.Http;
using NeonMika.Webserver.EventArgs;
using NetMf.CommonExtensions;

namespace CodingSmackdown.BrewController
{
    public class UpdateMashProfileMethod
    {
        public static bool UpdateMashProfile(RequestReceivedEventArgs e, JsonArray h)
        {
            try
            {
                string[] steps = null;
                string[] stepData = null;
                float tempValue = 0.0F;
                MashStep newMashStep = null;

                PinManagement.mashSteps = new MashSteps();
                PinManagement.mashSteps.Steps = new ArrayList();

                // mashProfile=0:122:15,1:135:15,2:148:60,3:170:15,4:225:90,

                if (e.Request.GetArguments.Contains("mashProfile"))
                {
                    string temp = e.Request.GetArguments["mashProfile"].ToString();
                    temp = temp.Replace("%3A", ":");
                    temp = temp.Replace("%2C", ",");

                    steps = temp.Split(',');

                    if (steps != null)
                    {
                        foreach (string step in steps)
                        {
                            stepData = step.Split(':');
                            if ((stepData != null) && (stepData.Length > 2))
                            {
                                newMashStep = new MashStep();
                                Settings.TryParseFloat(stepData[1], out tempValue);
                                newMashStep.StepNumber = Convert.ToInt32(stepData[0]); ;
                                newMashStep.Temperature = tempValue;
                                newMashStep.Time = Convert.ToInt32(stepData[2]);
                                PinManagement.mashSteps.Steps.Add(newMashStep);
                            }
                        }

                        PinManagement.mashSteps.CurrentStep = null;
                    }
                }

                // send back an ok response
                h.Add("OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                h.Add("ERROR");
                h.Add(ex.Message);
            }

            return true;
        }
    }
}