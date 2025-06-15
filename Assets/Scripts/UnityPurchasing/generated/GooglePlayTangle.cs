// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("71hrmL+FPLBlVQ8Lmqoq0LiYV/jlZmhnV+VmbWXlZmZn6OhDqqXrrmxnt4YU0Zq2ofkTftiijmQpAyQtV+VmRVdqYW5N4S/hkGpmZmZiZ2RsjIZ+U7SGJbZkhAtRAe3x4x6EFvVwN8O4+2cDt1t7Y6ofnnWbufaeTFWE07RERS5sjf0LLPaleNmp1taAebN7d4ojFttYFllvOxga+vDDapFfOfPmku4XbgpPhJ2OL+kyv/gi0lVdMdOs8sqObi2PrNVY3UN3GvHcdOk579kEpcZ2MNGfNOinYo9MGTKo2hr33KFpehuHbQ14blEj0DXxKuxMFWR1BoRzJmnOqDTfP79CN/xR/Wz2WcShPIG/pcrcNcJuEWD8EuYPStBKUjoDIGVkZmdm");
        private static int[] order = new int[] { 13,1,7,13,5,13,12,8,11,12,10,11,13,13,14 };
        private static int key = 103;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
