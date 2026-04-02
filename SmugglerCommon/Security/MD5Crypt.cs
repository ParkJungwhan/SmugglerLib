using System.Security.Cryptography;
using System.Text;

namespace Smuggler.Common.Security;

/// <summary>
/// MD5 해시 유틸리티.
/// </summary>
/// <remarks>
/// <b>⚠ 보안 경고:</b><br/>
/// MD5는 암호학적으로 취약한 해시 알고리즘으로, 충돌 공격이 알려져 있다.<br/>
/// <b>사용 가능한 용도:</b> 파일 무결성 체크섬, 캐시 키 생성 등 비보안 목적.<br/>
/// <b>사용 금지 용도:</b> 비밀번호 해싱, 전자서명, 보안 토큰 생성.<br/>
/// <br/>
/// <b>보안 목적 대체 해시:</b><br/>
/// - <c>SHA256.HashData(bytes)</c> : 범용 암호화 해시 (권장)<br/>
/// - <c>SHA512.HashData(bytes)</c> : 더 높은 보안이 필요한 경우<br/>
/// - <c>System.Security.Cryptography.RandomNumberGenerator</c> : 보안 난수/토큰 생성<br/>
/// - ASP.NET Identity의 PasswordHasher : 비밀번호 해싱
/// </remarks>
public static class MD5Crypt
{
    /// <summary>
    /// input 문자열의 MD5 해시를 대문자 16진수 문자열로 반환한다.<br/>
    /// <b>체크섬 / 비보안 목적으로만 사용할 것.</b>
    /// </summary>
    /// <exception cref="ArgumentNullException">input이 null일 때.</exception>
    public static string CreateMD5(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder(hashBytes.Length * 2);
        for (int i = 0; i < hashBytes.Length; i++)
            sb.Append(hashBytes[i].ToString("X2"));

        return sb.ToString();
    }
}
