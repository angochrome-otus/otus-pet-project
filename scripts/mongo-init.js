// MongoDB initialization script
// This script will be executed when MongoDB container starts for the first time

// Create database
db = db.getSiblingDB('SteelDesignerDb');

// Create collections with validation
db.createCollection('Users', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['Email', 'Role', 'CreatedAt'],
      properties: {
        Email: {
          bsonType: 'string',
          description: 'User email address'
        },
        Role: {
          bsonType: 'string',
          enum: ['Student', 'Teacher', 'Admin'],
          description: 'User role'
        },
        CreatedAt: {
          bsonType: 'date',
          description: 'User creation date'
        }
      }
    }
  }
});

db.createCollection('SessionHistory', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['UserId', 'SessionId', 'LoginTime'],
      properties: {
        UserId: {
          bsonType: 'string',
          description: 'User identifier'
        },
        SessionId: {
          bsonType: 'string',
          description: 'Session identifier'
        },
        LoginTime: {
          bsonType: 'date',
          description: 'Login timestamp'
        }
      }
    }
  }
});

db.createCollection('PageContent', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['PageKey', 'Content'],
      properties: {
        PageKey: {
          bsonType: 'string',
          description: 'Page unique key'
        },
        Content: {
          bsonType: 'string',
          description: 'Page content'
        }
      }
    }
  }
});

// Create indexes
db.Users.createIndex({ 'Email': 1 }, { unique: true });
db.Users.createIndex({ 'Role': 1 });
db.Users.createIndex({ 'CreatedAt': -1 });

db.SessionHistory.createIndex({ 'UserId': 1 });
db.SessionHistory.createIndex({ 'SessionId': 1 });
db.SessionHistory.createIndex({ 'LoginTime': -1 });

db.PageContent.createIndex({ 'PageKey': 1 }, { unique: true });

print('MongoDB initialization completed successfully');
