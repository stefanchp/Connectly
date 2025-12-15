# Connectly - a micro-social platform developed in .NET 

>[!warning]
> Still in development.

## Local setup (fresh clone)

1) Clone and enter the repo  
```bash
git clone https://github.com/stefanchp/Connectly.git
cd Connectly
```

2) Configure environment  
```bash
cp .env.example .env   # set DB_PASSWORD inside
```

3) Start SQL Server in Docker  
```bash
docker compose up -d db
```

4) Apply migrations (creates the schema and seeds users/groups)  
```bash
dotnet ef database update
```

5) Run the app  
```bash
dotnet run
```

Seeded accounts (for testing):
- admin@test.com / Admin1! (Admin)
- maria@example.com / User1!
- andrei@example.com / User2!

## Database Diagram

```mermaid
erDiagram
    AspNetUsers {
        string Id PK
        string FullName
        string Bio
        string ProfileImageUrl
        bool IsPrivate
        string UserName
        string NormalizedUserName
        string Email
    }
    FollowRequests {
        int Id PK
        string FromUserId FK
        string ToUserId FK
        int Status
        datetime CreatedAt
        datetime RespondedAt
    }
    Groups {
        int Id PK
        string Name
        string Description
        string CreatedById FK
        datetime CreatedAt
    }
    GroupMembers {
        int Id PK
        int GroupId FK
        string UserId FK
        int Role
        int Status
        datetime CreatedAt
    }
    GroupMessages {
        int Id PK
        int GroupId FK
        string UserId FK
        string Content
        datetime CreatedAt
    }

    AspNetUsers ||--o{ FollowRequests : "from"
    AspNetUsers ||--o{ FollowRequests : "to"
    AspNetUsers ||--o{ Groups : "creates"
    AspNetUsers ||--o{ GroupMembers : "joins"
    AspNetUsers ||--o{ GroupMessages : "writes"
    Groups ||--o{ GroupMembers : "has members"
    Groups ||--o{ GroupMessages : "has messages"
```
