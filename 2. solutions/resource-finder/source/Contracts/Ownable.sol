pragma solidity ^0.5.1;

contract Ownable
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