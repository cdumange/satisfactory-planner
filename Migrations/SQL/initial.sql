CREATE TABLE resources (
    id uuid PRIMARY KEY,
    name text UNIQUE NOT NULL
);

CREATE TABLE recipes (
    id uuid PRIMARY KEY,
    Name text NOT NULL UNIQUE,
    output_id uuid NOT NULL,
    FOREIGN KEY(output_id) REFERENCES resources(id) ON DELETE CASCADE,
    quantity int NOT NULL
);

CREATE TABLE ingredients (
    resource_id uuid NOT NULL,
    recipe_id uuid NOT NULL,
    quantity int NOT NULL,

    FOREIGN KEY (resource_id) REFERENCES resources(id) ON DELETE CASCADE,
    FOREIGN KEY (recipe_id) REFERENCES recipes(id) ON DELETE CASCADE,

    UNIQUE(resource_id, recipe_id)
);
