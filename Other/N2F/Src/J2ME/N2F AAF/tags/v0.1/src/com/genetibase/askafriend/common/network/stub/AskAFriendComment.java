// This class was generated by the JAXRPC SI, do not edit.
// Contents subject to change without notice.
// JSR-172 Reference Implementation wscompile 1.0, using: JAX-RPC Standard Implementation (1.1, build R59)

package com.genetibase.askafriend.common.network.stub;


public class AskAFriendComment {
    protected java.lang.String nickName;
    protected java.lang.String webMemberID;
    protected java.lang.String webAskAFriendCommentID;
    protected java.lang.String text;
    protected java.lang.String dateTimePosted;
    
    public AskAFriendComment() {
    }
    
    public AskAFriendComment(java.lang.String nickName, java.lang.String webMemberID, java.lang.String webAskAFriendCommentID, java.lang.String text, java.lang.String dateTimePosted) {
        this.nickName = nickName;
        this.webMemberID = webMemberID;
        this.webAskAFriendCommentID = webAskAFriendCommentID;
        this.text = text;
        this.dateTimePosted = dateTimePosted;
    }
    
    public java.lang.String getNickName() {
        return nickName;
    }
    
    public void setNickName(java.lang.String nickName) {
        this.nickName = nickName;
    }
    
    public java.lang.String getWebMemberID() {
        return webMemberID;
    }
    
    public void setWebMemberID(java.lang.String webMemberID) {
        this.webMemberID = webMemberID;
    }
    
    public java.lang.String getWebAskAFriendCommentID() {
        return webAskAFriendCommentID;
    }
    
    public void setWebAskAFriendCommentID(java.lang.String webAskAFriendCommentID) {
        this.webAskAFriendCommentID = webAskAFriendCommentID;
    }
    
    public java.lang.String getText() {
        return text;
    }
    
    public void setText(java.lang.String text) {
        this.text = text;
    }
    
    public java.lang.String getDateTimePosted() {
        return dateTimePosted;
    }
    
    public void setDateTimePosted(java.lang.String dateTimePosted) {
        this.dateTimePosted = dateTimePosted;
    }
}
