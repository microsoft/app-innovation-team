pragma solidity ^0.5.1;

contract OwnerContract
{
    address internal owner;
    
    modifier onlyOwner() {
        require(isOwner(msg.sender));
        _;
    }

    constructor() public {
        owner = msg.sender;
    }

    function isOwner(address addr) view internal returns(bool) {
        return addr == owner;
    }

    function transferOwnership(address newOwner) internal onlyOwner {
        if (newOwner != address(this)) {
            owner = newOwner;
        }
    }
}

contract RFContract is OwnerContract
{
    uint documents;
    
    struct Document {
        
        //document attributes
        bytes32 id;
        uint status;
        bytes32 hash;
        
        //just to validate if record has been registered before
        bool registered;
    }

    mapping(bytes32 => Document) public Documents;
    
    function Register(bytes32 id, uint status, bytes32 hash) public onlyOwner {
        
        //validate if record has been registered before
        require(!Documents[id].registered);
        
        //update document attributes
        Documents[id].id = id;
        Documents[id].status = status;
        Documents[id].hash = hash;
        
        //update registered flag in document
        Documents[id].registered = true;
        
        //increase documents counter
        documents++;
	}
	
	function GetRecord(bytes32 id) public view onlyOwner returns (bytes32, uint, bytes32) {
	    Document storage document = Documents[id];
	    return (document.id, document.status, document.hash);
	}
}