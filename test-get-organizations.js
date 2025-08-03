// Test script to get all organizations
const API_BASE = 'http://localhost:5181';

async function testGetOrganizations() {
    console.log('📋 Testing Get All Organizations...\n');

    try {
        const response = await fetch(`${API_BASE}/Org/GetAllOrganization`);
        
        console.log(`Status: ${response.status} ${response.statusText}`);
        
        if (response.ok) {
            const result = await response.json();
            console.log('✅ Success! Response:', JSON.stringify(result, null, 2));
            
            if (result.success && result.data && result.data.items) {
                console.log(`\n📊 Found ${result.data.items.length} organizations:`);
                result.data.items.forEach((org, index) => {
                    console.log(`${index + 1}. ${org.name} (${org.teamShortName || 'No short name'})`);
                    console.log(`   - ID: ${org.id}`);
                    console.log(`   - League ID: ${org.leagueId}`);
                    console.log(`   - Sport: ${org.sport || 'Not specified'}`);
                    console.log(`   - Stadium: ${org.venue?.stadium || 'Not specified'}`);
                    console.log(`   - Location: ${org.venue?.location || 'Not specified'}`);
                    console.log('');
                });
            } else {
                console.log('⚠️ No organizations found or unexpected response format');
            }
        } else {
            const errorText = await response.text();
            console.log('❌ Failed! Error response:', errorText);
        }
    } catch (error) {
        console.log('❌ Network Error:', error.message);
    }
}

// Run the test
testGetOrganizations().catch(console.error);
