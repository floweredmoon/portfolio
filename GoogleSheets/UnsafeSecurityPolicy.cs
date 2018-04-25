using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

public class UnsafeSecurityPolicy
{
    public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
    {
        Debug.Log("Validation successful!");
        return true;
    }

    public static void Instate()
    {
        ServicePointManager.ServerCertificateValidationCallback = Validator;
    }
}
