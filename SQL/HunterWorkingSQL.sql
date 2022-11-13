CREATE DATABASE library;

SELECT current_date() FROM dual;

USE library;

CREATE TABLE tPermissionLookup(
	iID INT PRIMARY KEY,
    sName VARCHAR(10)
);

INSERT INTO tPermissionLookup (iID, sName) VALUES (0, 'Owner'), (1, 'Editor'), (2, 'Viewer'), (3, 'None');
SELECT * FROM tPermissionLookup;

SELECT * FROM AspNetUsers;


CREATE TABLE tLibrary(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    sName VARCHAR(80),
    dtCreated DATETIME DEFAULT CURRENT_TIMESTAMP
);
ALTER TABLE tLibrary ADD COLUMN sOwner VARCHAR(256);

CREATE TABLE tPermission(
	sUserID VARCHAR(256) NOT NULL,
    iLibraryID INT NOT NULL,
    iPermissionLevel INT DEFAULT 0,
    FOREIGN KEY (sUserID) REFERENCES AspNetUsers (Id),
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID)
);
    
CREATE TABLE tCollection(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    iLibraryID INT NOT NULL,
    iParentCollectionID INT,
    sName VARCHAR(80),
    sDescription VARCHAR(255),
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID)
);

CREATE TABLE tBook(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    iLibraryID INT NOT NULL,
    sTitle VARCHAR(255),
    sSynopsis VARCHAR(1023),
    dtAdded DATETIME,
    dtPublished DATETIME,
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID)
);

CREATE TABLE tAuthor(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    sFirstName VARCHAR(40),
    sLastName VARCHAR(40)
);

CREATE TABLE tBookAuthorXREF(
	iBookID INT NOT NULL,
    iAuthorID INT NOT NULL,
    iListPosition INT,
    FOREIGN KEY (iBookID) REFERENCES tBook (iID),
    FOREIGN KEY (iAuthorID) REFERENCES tAuthor (iID),
    PRIMARY KEY (iBookID, iAuthorID)
);

CREATE TABLE tCollectionBookXREF(
	iCollectionID INT NOT NULL,
    iBookID INT NOT NULL,
    FOREIGN KEY (iCollectionID) REFERENCES tCollection (iID),
    FOREIGN KEY (iBookID) REFERENCES tBook (iID),
    PRIMARY KEY (iCollectionID, iBookID)
);


CREATE TABLE tLibraryInvite(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    iLibraryID INT NOT NULL,
    sInviterID VARCHAR(256) NOT NULL,
    sRecipientID VARCHAR(256) NOT NULL,
    iPermissionLevel INT,
    dtSent DATETIME,
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID),
    FOREIGN KEY (sInviterID) REFERENCES AspNetUsers (Id),
    FOREIGN KEY (sRecipientID) REFERENCES AspNetUsers (Id)
);

DROP TABLE tLibraryInvite;


SELECT Id, UserName FROM AspNetUsers WHERE UserName LIKE '%h%';

SELECT * FROM tLibrary;

SELECT * FROM tLibraryInvite;
SELECT i.*, s.UserName as 'sInviterUsername', r.UserName as 'sRecipientUsername', l.sName as 'sLibraryName' FROM tLibraryInvite i INNER JOIN AspNetUsers s ON i.sInviterID=s.Id INNER JOIN AspNetUsers r ON i.sRecipientID=s.Id INNER JOIN tLibrary l on i.iLibraryID=l.iID;

SELECT p.*, u.username FROM tPermission p INNER JOIN AspNetUsers u ON p.sUserID=u.Id WHERE p.iLibraryID=1;


SELECT i.*, s.UserName as 'sInviterUsername', r.UserName as 'sRecipientUsername', l.sName as 'sLibraryName' FROM tLibraryInvite i LEFT OUTER JOIN AspNetUsers s ON i.sInviterID=s.Id LEFT OUTER JOIN AspNetUsers r ON i.sRecipientID=r.Id LEFT OUTER JOIN tLibrary l on i.iLibraryID=l.iID;


SELECT * FROM tLibrary;
SELECT * FROM tLibraryInvite;

ALTER TABLE tCollection ADD COLUMN bUserModifiable BOOLEAN DEFAULT 1;
ALTER TABLE tLibrary ADD COLUMN iDefaultCollectionID INT NULL;

UPDATE tCollection SET bUserModifiable=1 WHERE iLibraryID=1;
SELECT * FROM tCollection;
UPDATE tLibrary SET iDefaultCollectionID=1 WHERE iID=1;
SELECT * FROM tLibrary;

CREATE TABLE tPasswordResetCodes(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    sUserID VARCHAR(256) NOT NULL,
    sHash NVARCHAR(256),
    sSalt NVARCHAR(256),
    dtExpires DATETIME,
    FOREIGN KEY (sUserID) REFERENCES AspNetUsers (Id)
);

CREATE TABLE tEmailVerificationCodes(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    sUserID VARCHAR(256) NOT NULL,
    sCode NVARCHAR(10),
    dtSent DATETIME,
    bVerified BOOLEAN,
    FOREIGN KEY (sUserID) REFERENCES AspNetUsers (Id)
);

CREATE TABLE tTag(
	iID INT PRIMARY KEY AUTO_INCREMENT,
    iLibraryID INT NOT NULL,
    sName NVARCHAR(30),
    FOREIGN KEY (iLibraryID) REFERENCES tLibrary (iID)
);

CREATE TABLE tBookTagXREF(
	iBookID INT NOT NULL,
    iTagID INT NOT NULL,
    FOREIGN KEY (iBookID) REFERENCES tBook (iID),
    FOREIGN KEY (iTagID) REFERENCES tTag (iID)
);
