// This class was generated by the JAXRPC SI, do not edit.
// Contents subject to change without notice.
// JSR-172 Reference Implementation wscompile 1.0, using: JAX-RPC Standard Implementation (1.1, build R59)

package n2f.tag.webservice.stub;


public class GetMemberID {
    protected java.lang.String nickName;
    protected java.lang.String webPassword;
    
    public GetMemberID() {
    }
    
    public GetMemberID(java.lang.String nickName, java.lang.String webPassword) {
        this.nickName = nickName;
        this.webPassword = webPassword;
    }
    
    public java.lang.String getNickName() {
        return nickName;
    }
    
    public void setNickName(java.lang.String nickName) {
        this.nickName = nickName;
    }
    
    public java.lang.String getWebPassword() {
        return webPassword;
    }
    
    public void setWebPassword(java.lang.String webPassword) {
        this.webPassword = webPassword;
    }
}
