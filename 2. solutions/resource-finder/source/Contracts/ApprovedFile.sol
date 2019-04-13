pragma solidity ^0.5.1;
import "./Ownable.sol";

contract ApprovedFile is Ownable
{
    uint documents;
   
    event NewDocumentRegistered(bytes32 id, string description, bytes32 hash, address sender);

    struct Document {
        
        //document attributes
        bytes32 id;
        string description;
        bytes32 hash;
        
        //just to validate if record has been registered before
        bool registered;
    }

    mapping(bytes32 => Document) public Documents;
    
    function Register(bytes32 id, string memory description, bytes32 hash) public onlyOwner {
        
        //validate if record has been registered before
        require(!Documents[id].registered);
        
        //update document attributes
        Documents[id].id = id;
        Documents[id].description = description;
        Documents[id].hash = hash;
        
        //update registered flag in document
        Documents[id].registered = true;
        
        emit NewDocumentRegistered(id,description,hash,msg.sender);

        //increase documents counter
        documents++;
	}
	
	function GetRecord(bytes32 id) public view onlyOwner returns (bytes32, string memory, bytes32) {
	    Document storage document = Documents[id];
	    return (document.id, document.description, document.hash);
	}
}