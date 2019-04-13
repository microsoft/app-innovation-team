pragma solidity ^0.5.1;
import "./Ownable.sol";

contract NetworkParticipants is Ownable
{

    event NodeAddedToNetwork(string nodeName, string nodePublicKey, address sender);
    event NodeRemovedFromNetwork(string nodeName);

    mapping(string => string) public NetworkNamesAndKeys;

    function AddNetworkParticipant(string memory orgName, string memory nodePublicKey) public {

        NetworkNamesAndKeys[orgName] = nodePublicKey;
        emit NodeAddedToNetwork(orgName,nodePublicKey,msg.sender);

    }

    function GetNodePublicKey(string memory orgName) public view returns (string memory){
        
        bytes memory tempString = bytes(NetworkNamesAndKeys[orgName]);

        if(tempString.length == 0){
            revert();
        }
        return NetworkNamesAndKeys[orgName];
    }

    function RemoveNetworkParticipant(string memory orgName) public onlyOwner {
        NetworkNamesAndKeys[orgName] = "";
        emit NodeRemovedFromNetwork(orgName);
    }
}